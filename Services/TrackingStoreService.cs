using System.Text.Json;
using IntelcomTracker.Models;

namespace IntelcomTracker.Services;

public interface ITrackingStoreService
{
    TrackingStore Load();
    void Save(TrackingStore store);
    string StorePath { get; }
}

public class TrackingStoreService : ITrackingStoreService
{
    private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions _readOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IntelcomTracker");

    public string StorePath { get; }

    public TrackingStoreService(string? storePath = null)
    {
        var appDataRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppDataDir));
        var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        var resolved = Path.GetFullPath(storePath ?? Path.Combine(appDataRoot, "tracking.json"));

        // Normalize, then confirm the resolved path stays within a trusted root
        // (app-data dir, or the temp dir used by tests) before any file I/O.
        if (!resolved.StartsWith(appDataRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !resolved.StartsWith(tempRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Store path must be within the application data or temp directory.",
                nameof(storePath));
        }

        StorePath = resolved;
    }

    public TrackingStore Load()
    {
        if (!File.Exists(StorePath)) return new TrackingStore();
        try
        {
            var json = File.ReadAllText(StorePath);
            return JsonSerializer.Deserialize<TrackingStore>(json, _readOptions) ?? new TrackingStore();
        }
        catch { return new TrackingStore(); }
    }

    public void Save(TrackingStore store)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            File.WriteAllText(StorePath, JsonSerializer.Serialize(store, _writeOptions));
        }
        catch { }
    }
}
