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
    [DataRow(null, "restaurant,cafe,fast_food_restaurant,food_court,bakery,meal_takeaway")]
    [DataRow("taiwanese-chinese", "taiwanese_restaurant,chinese_restaurant")]
    [DataRow("japanese", "japanese_restaurant,sushi_restaurant,ramen_restaurant")]
    [DataRow("korean", "korean_restaurant,korean_barbecue_restaurant")]
    [DataRow("hot-pot", "hot_pot_restaurant")]
    [DataRow("barbecue", "barbecue_restaurant,yakiniku_restaurant")]
    [DataRow("italian", "italian_restaurant,pizza_restaurant")]
    [DataRow("breakfast-brunch", "breakfast_restaurant,brunch_restaurant")]
    [DataRow("fast-food", "fast_food_restaurant,hamburger_restaurant")]
    [DataRow("vegetarian", "vegetarian_restaurant,vegan_restaurant")]
    [DataRow("cafe-dessert", "cafe,coffee_shop,dessert_shop,dessert_restaurant")]
    public async Task Category_mapping_uses_one_bounded_google_request(string? category, string expectedTypes)
    {
        var request = JsonSerializer.Deserialize<EatThis.Api.Contracts.NearbyFoodRequest>(
            JsonSerializer.Serialize(new { latitude = 25.033, longitude = 121.5654, radiusMeters = 1000, minRating = 4, restaurantCategory = category }),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.IsTrue(NearbySearchQuery.TryCreate(request, out var query));
        var handler = new RecordingHandler("{\"places\":[]}");
        using var client = new HttpClient(handler) { BaseAddress = GooglePlacesProvider.DefaultBaseAddress };
        var provider = new GooglePlacesProvider(client, Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }), NullLogger<GooglePlacesProvider>.Instance);
        var candidates = await provider.SearchAsync(query, CancellationToken.None);
        Assert.AreEqual(0, candidates.Count);
        Assert.AreEqual(1, handler.CallCount);
        using var json = JsonDocument.Parse(handler.RequestBody!);
        var root = json.RootElement;
        CollectionAssert.AreEqual(expectedTypes.Split(','), root.GetProperty("includedTypes").EnumerateArray().Select(value => value.GetString()).ToArray());
        Assert.AreEqual("zh-TW", root.GetProperty("languageCode").GetString());
        Assert.AreEqual(20, root.GetProperty("maxResultCount").GetInt32());
        var circle = root.GetProperty("locationRestriction").GetProperty("circle");
        Assert.AreEqual(1000d, circle.GetProperty("radius").GetDouble());
        Assert.AreEqual(25.033, circle.GetProperty("center").GetProperty("latitude").GetDouble());
        Assert.AreEqual(121.5654, circle.GetProperty("center").GetProperty("longitude").GetDouble());
        Assert.AreEqual("places.displayName,places.formattedAddress,places.location,places.googleMapsUri,places.rating", handler.Request!.Headers.GetValues("X-Goog-FieldMask").Single());
        foreach (var property in new[] { "minRating", "includedPrimaryTypes", "excludedTypes", "fieldMask" })
            Assert.IsFalse(root.TryGetProperty(property, out _));
    }

    [TestMethod]
    [DataRow("", null)]
    [DataRow(",\"rating\":null", null)]
    [DataRow(",\"rating\":0", null)]
    [DataRow(",\"rating\":0.5", null)]
    [DataRow(",\"rating\":5.1", null)]
    [DataRow(",\"rating\":1e400", null)]
    [DataRow(",\"rating\":1", 1.0)]
    [DataRow(",\"rating\":4.3", 4.3)]
    [DataRow(",\"rating\":5", 5.0)]
    public async Task Google_rating_is_normalized_without_removing_usable_places(string ratingJson, double? expected)
    {
        var handler = new RecordingHandler("{\"places\":[{\"displayName\":{\"text\":\"Food\"},\"formattedAddress\":\"Taipei\",\"location\":{\"latitude\":25.033,\"longitude\":121.5654},\"googleMapsUri\":\"https://example.com/food\"" + ratingJson + "}]}");
        using var client = new HttpClient(handler) { BaseAddress = GooglePlacesProvider.DefaultBaseAddress };
        var provider = new GooglePlacesProvider(client, Options.Create(new GooglePlacesOptions { ApiKey = "fake-test-key" }), NullLogger<GooglePlacesProvider>.Instance);
        var candidates = await provider.SearchAsync(new NearbySearchQuery(25.033, 121.5654, 700, 4.5), CancellationToken.None);
        Assert.AreEqual(1, candidates.Count);
        Assert.AreEqual(expected, candidates[0].Rating);
        Assert.IsFalse(handler.RequestBody!.Contains("minRating", StringComparison.Ordinal));
        Assert.IsFalse(handler.RequestBody.Contains("fake-test-key", StringComparison.Ordinal));
    }
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
            "places.displayName,places.formattedAddress,places.location,places.googleMapsUri,places.rating",
            handler.Request.Headers.GetValues("X-Goog-FieldMask").Single());

        using var requestJson = JsonDocument.Parse(handler.RequestBody!);
        var root = requestJson.RootElement;
        Assert.IsFalse(root.TryGetProperty("minRating", out _));
        Assert.AreEqual("zh-TW", root.GetProperty("languageCode").GetString());
        Assert.AreEqual(20, root.GetProperty("maxResultCount").GetInt32());
        CollectionAssert.AreEquivalent(
            new[] { "restaurant", "cafe", "fast_food_restaurant", "food_court", "bakery", "meal_takeaway" },
            root.GetProperty("includedTypes").EnumerateArray().Select(value => value.GetString()).ToArray());
        Assert.AreEqual(
            3000,
            root.GetProperty("locationRestriction").GetProperty("circle").GetProperty("radius").GetDouble());
    }

    [TestMethod]
    public async Task Nearby_search_preserves_non_empty_latin_fallback_when_localized_name_is_unavailable()
    {
        var handler = new RecordingHandler(
            """
            {
              "places": [
                {
                  "displayName": { "text": "Chin Huajiao Banqiao Xianmin Boulevard" },
                  "formattedAddress": "No. 1, Section 2, Xianmin Boulevard, Banqiao District",
                  "location": { "latitude": 25.0331, "longitude": 121.5655 },
                  "googleMapsUri": "https://www.google.com/maps/place/fallback-example"
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
        Assert.AreEqual("Chin Huajiao Banqiao Xianmin Boulevard", candidates[0].Name);
        Assert.AreEqual(
            "No. 1, Section 2, Xianmin Boulevard, Banqiao District",
            candidates[0].Address);
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
        public int CallCount { get; private set; }

        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
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
