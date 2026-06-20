using System.Net;
using System.Text;
using IntelcomTracker.Services;
using Xunit;

namespace IntelcomTracker.Tests;

public class IntelcomApiClientTests
{
    // --- Fake transport ---

    private class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;
        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            return Task.FromResult(_handler(request, ct));
        }
    }

    private static (IntelcomApiClient client, FakeHandler handler) Build(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
    {
        var fake = new FakeHandler(handler);
        return (new IntelcomApiClient(new HttpClient(fake)), fake);
    }

    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    // A realistic payload exercising the snake_case [JsonPropertyName] mappings.
    private const string FullPayload = """
    {
      "success": true,
      "data": {
        "code": "200",
        "result": {
          "tracking_id": "ABC123",
          "eta": "2026-06-21",
          "public_eta": { "from": "09:00", "to": "12:00" },
          "driver_name": "Pat",
          "last_status": {
            "timestamp": 1718900000,
            "statusCode": 450,
            "label": "Delivered",
            "labels": { "en": { "shortLabel": "Delivered", "longLabel": "Package delivered" } },
            "package_location": {
              "address": { "city": "Montreal", "state_province": "QC", "timezone": "America/Toronto" }
            },
            "isDelivered": true
          },
          "status_list": [
            { "timestamp": 1718800000, "statusCode": 105, "label": "In transit", "isDelivered": false }
          ]
        }
      }
    }
    """;

    // --- Tests ---

    [Fact]
    public async Task GetTrackingAsync_Success_DeservesAndMapsAllFields()
    {
        var (client, _) = Build((_, _) => Json(FullPayload));

        var result = await client.GetTrackingAsync("ABC123");

        Assert.NotNull(result);
        Assert.Equal("ABC123", result!.TrackingId);
        Assert.Equal("2026-06-21", result.Eta);
        Assert.Equal("09:00", result.PublicEta?.From);
        Assert.Equal("Pat", result.DriverName);

        var last = result.LastStatus;
        Assert.NotNull(last);
        Assert.Equal(450, last!.StatusCode);
        Assert.True(last.IsDelivered);
        Assert.Equal("Delivered", last.Labels?.En?.ShortLabel);
        Assert.Equal("Montreal", last.PackageLocation?.Address?.City);
        Assert.Equal("QC", last.PackageLocation?.Address?.StateProvince);

        var evt = Assert.Single(result.StatusList);
        Assert.Equal(105, evt.StatusCode);
        Assert.False(evt.IsDelivered);
    }

    [Fact]
    public async Task GetTrackingAsync_BuildsRequestUrlAndHeaders()
    {
        var (client, handler) = Build((_, _) => Json(FullPayload));

        await client.GetTrackingAsync("AB CD/12"); // chars that must be URL-escaped

        var req = handler.LastRequest!;
        Assert.Equal(HttpMethod.Get, req.Method);
        Assert.Equal("https://intelcom.ca/cfworker/v3/tracking/AB%20CD%2F12/", req.RequestUri!.AbsoluteUri);
        Assert.True(req.Headers.Contains("user-agent"));
        Assert.True(req.Headers.Contains("referer"));
    }

    [Fact]
    public async Task GetTrackingAsync_NonSuccessStatus_ReturnsNull()
    {
        var (client, _) = Build((_, _) => Json("{}", HttpStatusCode.NotFound));

        Assert.Null(await client.GetTrackingAsync("X"));
    }

    [Fact]
    public async Task GetTrackingAsync_MalformedJson_ReturnsNull()
    {
        var (client, _) = Build((_, _) => Json("{ this is not json"));

        Assert.Null(await client.GetTrackingAsync("X"));
    }

    [Fact]
    public async Task GetTrackingAsync_EmptyResult_ReturnsNull()
    {
        // Valid wrapper but no result payload -> null.
        var (client, _) = Build((_, _) => Json("""{ "success": true, "data": { "code": "404" } }"""));

        Assert.Null(await client.GetTrackingAsync("X"));
    }

    [Fact]
    public async Task GetTrackingAsync_Cancellation_Propagates()
    {
        var (client, _) = Build((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Json(FullPayload);
        });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetTrackingAsync("X", cts.Token));
    }
}
