using System.Text.Json.Serialization;

namespace IntelcomTracker.Models;

public sealed record ApiResponseWrapper
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public ApiResponseData? Data { get; init; }
}

public sealed record ApiResponseData
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("result")]
    public TrackingResult? Result { get; init; }
}

public sealed record TrackingResult
{
    [JsonPropertyName("tracking_id")]
    public string TrackingId { get; init; } = "";

    [JsonPropertyName("eta")]
    public string? Eta { get; init; }

    [JsonPropertyName("public_eta")]
    public PublicEta? PublicEta { get; init; }

    [JsonPropertyName("driver_name")]
    public string? DriverName { get; init; }

    [JsonPropertyName("last_status")]
    public StatusEvent? LastStatus { get; init; }

    [JsonPropertyName("status_list")]
    public List<StatusEvent> StatusList { get; init; } = [];
}

public sealed record PublicEta
{
    [JsonPropertyName("from")]
    public string? From { get; init; }

    [JsonPropertyName("to")]
    public string? To { get; init; }
}

public sealed record StatusEvent
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("labels")]
    public StatusLabels? Labels { get; init; }

    [JsonPropertyName("package_location")]
    public PackageLocation? PackageLocation { get; init; }

    [JsonPropertyName("isDelivered")]
    public bool IsDelivered { get; init; }
}

public sealed record StatusLabels
{
    [JsonPropertyName("en")]
    public LocalizedLabel? En { get; init; }
}

public sealed record LocalizedLabel
{
    [JsonPropertyName("shortLabel")]
    public string? ShortLabel { get; init; }

    [JsonPropertyName("longLabel")]
    public string? LongLabel { get; init; }
}

public sealed record PackageLocation
{
    [JsonPropertyName("address")]
    public LocationAddress? Address { get; init; }
}

public sealed record LocationAddress
{
    [JsonPropertyName("city")]
    public string? City { get; init; }

    [JsonPropertyName("state_province")]
    public string? StateProvince { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }
}
