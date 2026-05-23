using IntelcomTracker.Models;
using IntelcomTracker.Services;
using Xunit;

namespace IntelcomTracker.Tests;

public class RefreshServiceTests
{
    // --- Fakes ---

    private class FakeApi : IIntelcomApiClient
    {
        private readonly Func<string, Task<TrackingResult?>> _handler;
        public FakeApi(Func<string, Task<TrackingResult?>> handler) => _handler = handler;
        public Task<TrackingResult?> GetTrackingAsync(string id, CancellationToken ct = default)
            => _handler(id);
    }

    private static FakeApi Returns(TrackingResult? result)
        => new(_ => Task.FromResult(result));

    private static FakeApi Throws(Exception ex)
        => new(_ => Task.FromException<TrackingResult?>(ex));

    private class FakePersistence : ITrackingStoreService
    {
        public int SaveCount { get; private set; }
        public TrackingStore Load() => new();
        public void Save(TrackingStore _) => SaveCount++;
        public string StorePath => "";
    }

    private static (RefreshService svc, FakePersistence store) Build(IIntelcomApiClient api)
    {
        var store = new FakePersistence();
        return (new RefreshService(api, store), store);
    }

    // --- Tests ---

    [Fact]
    public async Task Success_UpdatesCachedDataAndClearsError()
    {
        var result = new TrackingResult { TrackingId = "ABC" };
        var (svc, _) = Build(Returns(result));
        var pkg = new TrackedPackage { TrackingId = "ABC", LastError = "previous error" };
        var trackingStore = new TrackingStore { Packages = [pkg] };

        await svc.RefreshAllAsync(trackingStore, CancellationToken.None);

        Assert.Same(result, pkg.CachedData);
        Assert.Null(pkg.LastError);
        Assert.NotNull(pkg.LastRefreshed);
    }

    [Fact]
    public async Task NotFound_SetsLastErrorAndPreservesExistingCachedData()
    {
        var existing = new TrackingResult { TrackingId = "X" };
        var (svc, _) = Build(Returns(null));
        var pkg = new TrackedPackage { TrackingId = "X", CachedData = existing };
        var trackingStore = new TrackingStore { Packages = [pkg] };

        await svc.RefreshAllAsync(trackingStore, CancellationToken.None);

        Assert.Equal("Not found (404)", pkg.LastError);
        Assert.Same(existing, pkg.CachedData);  // cached data preserved on 404
        Assert.NotNull(pkg.LastRefreshed);
    }

    [Fact]
    public async Task NetworkException_SetsLastErrorWithMessage()
    {
        var (svc, _) = Build(Throws(new HttpRequestException("Network unreachable")));
        var pkg = new TrackedPackage { TrackingId = "Y" };
        var trackingStore = new TrackingStore { Packages = [pkg] };

        await svc.RefreshAllAsync(trackingStore, CancellationToken.None);

        Assert.Contains("Network unreachable", pkg.LastError);
        Assert.NotNull(pkg.LastRefreshed);
    }

    [Fact]
    public async Task MultiplePackages_OneFailure_OtherSucceeds()
    {
        var good = new TrackingResult { TrackingId = "GOOD" };
        var api = new FakeApi(id => id == "GOOD"
            ? Task.FromResult<TrackingResult?>(good)
            : Task.FromException<TrackingResult?>(new Exception("boom")));

        var (svc, _) = Build(api);
        var pkgGood = new TrackedPackage { TrackingId = "GOOD" };
        var pkgBad = new TrackedPackage { TrackingId = "BAD" };
        var trackingStore = new TrackingStore { Packages = [pkgGood, pkgBad] };

        await svc.RefreshAllAsync(trackingStore, CancellationToken.None);

        Assert.Same(good, pkgGood.CachedData);
        Assert.Null(pkgGood.LastError);
        Assert.NotNull(pkgBad.LastError);
        Assert.Null(pkgBad.CachedData);
    }

    [Fact]
    public async Task Save_CalledOnceRegardlessOfPackageCount()
    {
        var (svc, fakePersistence) = Build(Returns(null));
        var trackingStore = new TrackingStore
        {
            Packages = [
                new TrackedPackage { TrackingId = "A" },
                new TrackedPackage { TrackingId = "B" },
                new TrackedPackage { TrackingId = "C" },
            ]
        };

        await svc.RefreshAllAsync(trackingStore, CancellationToken.None);

        Assert.Equal(1, fakePersistence.SaveCount);
    }

    [Fact]
    public async Task Cancellation_PropagatesAndDoesNotCallSave()
    {
        using var cts = new CancellationTokenSource();
        var api = new FakeApi(async _ =>
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
            return null;
        });
        var (svc, fakePersistence) = Build(api);
        var trackingStore = new TrackingStore { Packages = [new TrackedPackage { TrackingId = "A" }] };

        cts.CancelAfter(50);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => svc.RefreshAllAsync(trackingStore, cts.Token));

        Assert.Equal(0, fakePersistence.SaveCount);
    }
}
