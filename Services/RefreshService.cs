using IntelcomTracker.Models;

namespace IntelcomTracker.Services;

public class RefreshService
{
    private readonly IIntelcomApiClient _api;
    private readonly ITrackingStoreService _persistence;

    public RefreshService(IIntelcomApiClient api, ITrackingStoreService persistence)
    {
        _api = api;
        _persistence = persistence;
    }

    public async Task RefreshAllAsync(TrackingStore store, CancellationToken ct)
    {
        var tasks = store.Packages.Select(p => RefreshOneAsync(p, ct)).ToList();
        await Task.WhenAll(tasks);
        _persistence.Save(store);
    }

    private async Task RefreshOneAsync(TrackedPackage pkg, CancellationToken ct)
    {
        try
        {
            var result = await _api.GetTrackingAsync(pkg.TrackingId, ct);
            pkg.LastRefreshed = DateTime.UtcNow;
            if (result != null)
            {
                pkg.CachedData = result;
                pkg.LastError = null;
            }
            else
            {
                pkg.LastError = "Not found (404)";
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            pkg.LastRefreshed = DateTime.UtcNow;
            pkg.LastError = ex.Message;
        }
    }
}
