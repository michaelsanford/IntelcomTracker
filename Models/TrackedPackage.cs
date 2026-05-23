namespace IntelcomTracker.Models;

public class TrackedPackage
{
    public string TrackingId { get; set; } = "";
    public string? Nickname { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastRefreshed { get; set; }
    public TrackingResult? CachedData { get; set; }
    public string? LastError { get; set; }
}

public class TrackingStore
{
    public List<TrackedPackage> Packages { get; set; } = [];
    public int RefreshIntervalSeconds { get; set; } = 3600;
}
