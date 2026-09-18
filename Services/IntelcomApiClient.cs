using System.Net.Http.Json;
using IntelcomTracker.Models;

namespace IntelcomTracker.Services;

public interface IIntelcomApiClient
{
    Task<TrackingResult?> GetTrackingAsync(string trackingId, CancellationToken ct = default);
}

public class IntelcomApiClient(HttpClient http) : IIntelcomApiClient
{
    private readonly HttpClient _http = http;

    public async Task<TrackingResult?> GetTrackingAsync(string trackingId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://intelcom.ca/cfworker/v3/tracking/{Uri.EscapeDataString(trackingId)}/");

        request.Headers.TryAddWithoutValidation("accept", "application/json, text/javascript, */*; q=0.01");
        request.Headers.TryAddWithoutValidation("referer",
            $"https://intelcom.ca/en/track-your-package/?tracking-id={trackingId}");
        request.Headers.TryAddWithoutValidation("user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0");
        request.Headers.TryAddWithoutValidation("dnt", "1");

        try
        {
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var wrapper = await response.Content.ReadFromJsonAsync(
                IntelcomJsonContext.Default.ApiResponseWrapper,
                ct);
            return wrapper?.Data?.Result;
        }
        catch (OperationCanceledException) { throw; }
        catch { return null; }
    }
}
