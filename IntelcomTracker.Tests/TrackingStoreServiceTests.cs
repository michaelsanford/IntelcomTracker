using IntelcomTracker.Models;
using IntelcomTracker.Services;
using Xunit;

namespace IntelcomTracker.Tests;

public class TrackingStoreServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"intelcom-test-{Guid.NewGuid()}.json");
    private readonly TrackingStoreService _svc;

    public TrackingStoreServiceTests() => _svc = new TrackingStoreService(_path);

    public void Dispose() { if (File.Exists(_path)) File.Delete(_path); }

    [Fact]
    public void Load_WhenFileMissing_ReturnsEmptyStore()
    {
        var store = _svc.Load();
        Assert.Empty(store.Packages);
        Assert.Equal(3600, store.RefreshIntervalSeconds);
    }

    [Fact]
    public void SaveLoad_RoundTripsTrackingIdAndNickname()
    {
        var store = new TrackingStore
        {
            Packages = [new TrackedPackage { TrackingId = "INTLCMI123", Nickname = "Test box" }]
        };
        _svc.Save(store);
        var loaded = _svc.Load();

        Assert.Single(loaded.Packages);
        Assert.Equal("INTLCMI123", loaded.Packages[0].TrackingId);
        Assert.Equal("Test box", loaded.Packages[0].Nickname);
    }

    [Fact]
    public void SaveLoad_RoundTripsRefreshInterval()
    {
        _svc.Save(new TrackingStore { RefreshIntervalSeconds = 120 });
        Assert.Equal(120, _svc.Load().RefreshIntervalSeconds);
    }

    [Fact]
    public void SaveLoad_PreservesNullNickname()
    {
        _svc.Save(new TrackingStore { Packages = [new TrackedPackage { TrackingId = "X", Nickname = null }] });
        Assert.Null(_svc.Load().Packages[0].Nickname);
    }

    [Fact]
    public void Load_WhenFileIsCorruptJson_ReturnsEmptyStore()
    {
        File.WriteAllText(_path, "{ not valid json }}}");
        var store = _svc.Load();
        Assert.Empty(store.Packages);
    }

    [Fact]
    public void Load_WhenFileIsEmpty_ReturnsEmptyStore()
    {
        File.WriteAllText(_path, "");
        var store = _svc.Load();
        Assert.Empty(store.Packages);
    }

    [Fact]
    public void Save_CreatesDirectoryIfMissing()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"intelcom-dir-{Guid.NewGuid()}");
        var svc = new TrackingStoreService(Path.Combine(dir, "tracking.json"));
        try
        {
            svc.Save(new TrackingStore());
            Assert.True(Directory.Exists(dir));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void Ctor_RejectsPathOutsideAllowedRoots() =>
        Assert.Throws<ArgumentException>(() =>
            new TrackingStoreService(Path.Combine(AppContext.BaseDirectory, "evil.json")));
}
