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
        var resolved = Path.GetFullPath(storePath ?? Path.Combine(AppDataDir, "tracking.json"));

        if (!IsWithin(resolved, AppDataDir) && !IsWithin(resolved, Path.GetTempPath()))
            throw new ArgumentException(
                "Store path must be within the application data or temp directory.",
                nameof(storePath));

        StorePath = resolved;
    }

    private static bool IsWithin(string path, string root)
    {
        root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        return path == root
            || path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal);
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
