using IntelcomTracker.Models;

namespace IntelcomTracker.Services;

public class RefreshService(
    IIntelcomApiClient api,
    ITrackingStoreService persistence,
    TimeProvider? timeProvider = null)
{
    private readonly IIntelcomApiClient _api = api;
    private readonly ITrackingStoreService _persistence = persistence;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

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
            pkg.LastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
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
            pkg.LastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
            pkg.LastError = ex.Message;
        }
    }
}
