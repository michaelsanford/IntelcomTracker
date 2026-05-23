using System.Text.Json.Serialization;

namespace IntelcomTracker.Models;

public class ApiResponseWrapper
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public ApiResponseData? Data { get; set; }
}

public class ApiResponseData
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("result")]
    public TrackingResult? Result { get; set; }
}

public class TrackingResult
{
    [JsonPropertyName("tracking_id")]
    public string TrackingId { get; set; } = "";

    [JsonPropertyName("eta")]
    public string? Eta { get; set; }

    [JsonPropertyName("public_eta")]
    public PublicEta? PublicEta { get; set; }

    [JsonPropertyName("driver_name")]
    public string? DriverName { get; set; }

    [JsonPropertyName("last_status")]
    public StatusEvent? LastStatus { get; set; }

    [JsonPropertyName("status_list")]
    public List<StatusEvent> StatusList { get; set; } = [];
}

public class PublicEta
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }
}

public class StatusEvent
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("labels")]
    public StatusLabels? Labels { get; set; }

    [JsonPropertyName("package_location")]
    public PackageLocation? PackageLocation { get; set; }

    [JsonPropertyName("isDelivered")]
    public bool IsDelivered { get; set; }
}

public class StatusLabels
{
    [JsonPropertyName("en")]
    public LocalizedLabel? En { get; set; }
}

public class LocalizedLabel
{
    [JsonPropertyName("shortLabel")]
    public string? ShortLabel { get; set; }

    [JsonPropertyName("longLabel")]
    public string? LongLabel { get; set; }
}

public class PackageLocation
{
    [JsonPropertyName("address")]
    public LocationAddress? Address { get; set; }
}

public class LocationAddress
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state_province")]
    public string? StateProvince { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}
