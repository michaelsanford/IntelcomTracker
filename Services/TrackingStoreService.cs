using System.Runtime.CompilerServices;
using System.Text.Json;
using IntelcomTracker.Models;

[assembly: InternalsVisibleTo("IntelcomTracker.Tests")]

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

    /// <summary>
    /// Production constructor: the store always lives at a fixed, trusted location
    /// under the user's local application data. No caller-supplied path is accepted,
    /// so there is no external path input to the file operations.
    /// </summary>
    public TrackingStoreService()
        : this(Path.Combine(AppDataDir, "tracking.json")) { }

    /// <summary>
    /// Test-only constructor. Kept <c>internal</c> (exposed to the test assembly via
    /// <see cref="InternalsVisibleTo"/>) so an arbitrary path is not part of the
    /// public API surface. The path is still normalized and confined to a trusted
    /// root as defense in depth.
    /// </summary>
    internal TrackingStoreService(string storePath)
    {
        var appDataRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppDataDir));
        var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        var resolved = Path.GetFullPath(storePath);

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
