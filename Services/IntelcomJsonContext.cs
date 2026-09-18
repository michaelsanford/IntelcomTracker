using System.Text.Json.Serialization;
using IntelcomTracker.Models;

namespace IntelcomTracker.Services;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ApiResponseWrapper))]
[JsonSerializable(typeof(TrackingStore))]
internal partial class IntelcomJsonContext : JsonSerializerContext;
