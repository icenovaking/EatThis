using System.Net;
using System.Text.Json;
using EatThis.Api.Domain;
using EatThis.Api.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class GooglePlacesProviderTests
{
    [TestMethod]
    public async Task Nearby_search_uses_fixed_request_shape_and_maps_google_fields()
    {
        var handler = new RecordingHandler(
            """
            {
              "places": [
                {
                  "displayName": { "text": "Google Food Shop" },
                  "formattedAddress": "Taipei City",
                  "location": { "latitude": 25.0331, "longitude": 121.5655 },
                  "googleMapsUri": "https://www.google.com/maps/place/example"
                }
              ]
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = GooglePlacesProvider.DefaultBaseAddress,
        };
        var provider = new GooglePlacesProvider(
            httpClient,
            Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }),
            NullLogger<GooglePlacesProvider>.Instance);

        var candidates = await provider.SearchAsync(
            new NearbySearchQuery(25.0330, 121.5654, 3000),
            CancellationToken.None);

        Assert.AreEqual(1, candidates.Count);
        Assert.AreEqual("Google Food Shop", candidates[0].Name);
        Assert.AreEqual("google", candidates[0].Provider);
        Assert.AreEqual("https://www.google.com/maps/place/example", candidates[0].NavigationUrl);
        Assert.IsTrue(candidates[0].DistanceMeters > 0);
        Assert.IsTrue(candidates[0].DistanceMeters < 100);

        Assert.IsNotNull(handler.Request);
        Assert.AreEqual(
            "https://places.googleapis.com/v1/places:searchNearby",
            handler.Request!.RequestUri!.ToString());
        Assert.AreEqual(
            "fake-test-key",
            handler.Request.Headers.GetValues("X-Goog-Api-Key").Single());
        Assert.AreEqual(
            "places.displayName,places.formattedAddress,places.location,places.googleMapsUri",
            handler.Request.Headers.GetValues("X-Goog-FieldMask").Single());

        using var requestJson = JsonDocument.Parse(handler.RequestBody!);
        var root = requestJson.RootElement;
        Assert.AreEqual(20, root.GetProperty("maxResultCount").GetInt32());
        CollectionAssert.Contains(
            root.GetProperty("includedTypes").EnumerateArray().Select(value => value.GetString()).ToArray(),
            "restaurant");
        Assert.AreEqual(
            3000,
            root.GetProperty("locationRestriction").GetProperty("circle").GetProperty("radius").GetDouble());
    }

    [TestMethod]
    public async Task Nearby_search_excludes_google_places_without_required_navigation_data()
    {
        var handler = new RecordingHandler(
            """
            {
              "places": [
                {
                  "displayName": { "text": "No Maps Link" },
                  "formattedAddress": "Taipei City",
                  "location": { "latitude": 25.0331, "longitude": 121.5655 }
                },
                {
                  "formattedAddress": "Taipei City",
                  "location": { "latitude": 25.0331, "longitude": 121.5655 },
                  "googleMapsUri": "https://www.google.com/maps/place/missing-name"
                }
              ]
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = GooglePlacesProvider.DefaultBaseAddress,
        };
        var provider = new GooglePlacesProvider(
            httpClient,
            Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }),
            NullLogger<GooglePlacesProvider>.Instance);

        var candidates = await provider.SearchAsync(
            new NearbySearchQuery(25.0330, 121.5654, 3000),
            CancellationToken.None);

        Assert.AreEqual(0, candidates.Count);
    }

    [TestMethod]
    public async Task Google_quota_failure_is_translated_without_exposing_provider_body()
    {
        var handler = new RecordingHandler(
            "upstream-secret-quota-body",
            HttpStatusCode.TooManyRequests);
        var logger = new RecordingLogger<GooglePlacesProvider>();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = GooglePlacesProvider.DefaultBaseAddress,
        };
        var provider = new GooglePlacesProvider(
            httpClient,
            Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }),
            logger);

        var exception = await Assert.ThrowsExceptionAsync<PlaceProviderException>(() =>
            provider.SearchAsync(
                new NearbySearchQuery(25.0330, 121.5654, 3000),
                CancellationToken.None));

        Assert.AreEqual(502, exception.StatusCode);
        Assert.IsFalse(exception.Message.Contains("upstream-secret", StringComparison.Ordinal));
        Assert.IsFalse(logger.Messages.Any(message =>
            message.Contains("fake-test-key", StringComparison.Ordinal) ||
            message.Contains("upstream-secret-quota-body", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task Provider_timeout_is_translated_to_service_unavailable()
    {
        using var httpClient = new HttpClient(new TimeoutHandler())
        {
            BaseAddress = GooglePlacesProvider.DefaultBaseAddress,
        };
        var provider = new GooglePlacesProvider(
            httpClient,
            Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }),
            NullLogger<GooglePlacesProvider>.Instance);

        var exception = await Assert.ThrowsExceptionAsync<PlaceProviderException>(() =>
            provider.SearchAsync(
                new NearbySearchQuery(25.0330, 121.5654, 3000),
                CancellationToken.None));

        Assert.AreEqual(503, exception.StatusCode);
    }

    private sealed class RecordingHandler(
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody),
            };
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException("upstream timeout detail"));
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
            NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
