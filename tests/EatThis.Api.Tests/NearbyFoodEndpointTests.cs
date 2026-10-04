using System.Net;
using System.Net.Http.Json;
using EatThis.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class NearbyFoodEndpointTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(1234)]
    [DataRow(2147483647)]
    public async Task Success_propagates_nullable_review_count_without_changing_selection(int? expected)
    {
        var candidate = System.Text.Json.JsonSerializer.Deserialize<PlaceCandidate>(
            System.Text.Json.JsonSerializer.Serialize(new { name = "Counted Food", address = "Taipei", latitude = 25.033, longitude = 121.5654, distanceMeters = 20, navigationUrl = "https://example.com/food", provider = "google", rating = 4.9, userRatingCount = expected }),
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
        var provider = new ReviewCountProvider(candidate);
        await using var factory = new ConfigurableApiFactory(provider);
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick", new { latitude = 25.033, longitude = 121.5654, radiusMeters = 700, minRating = 4.5 });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual("Counted Food", json.RootElement.GetProperty("name").GetString());
        Assert.AreEqual(4.9, json.RootElement.GetProperty("rating").GetDouble());
        var count = json.RootElement.GetProperty("userRatingCount");
        if (expected is int value) Assert.AreEqual(value, count.GetInt32());
        else Assert.AreEqual(System.Text.Json.JsonValueKind.Null, count.ValueKind);
        Assert.AreEqual(0, json.RootElement.GetProperty("resetNavigationUrls").GetArrayLength());
        Assert.AreEqual(1, provider.Calls);
    }

    private sealed class ReviewCountProvider(PlaceCandidate candidate) : EatThis.Api.Application.IPlaceProvider
    {
        public int Calls { get; private set; }
        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(EatThis.Api.Domain.NearbySearchQuery query, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<PlaceCandidate>>([candidate]);
        }
    }

    [TestMethod]
    public async Task Exhausted_singleton_returns_reset_keys_with_original_place_fields()
    {
        const string url = "https://www.google.com/maps/place/example";
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick", new {
            latitude = 25.033, longitude = 121.5654,
            excludedNavigationUrls = new[] { url.ToUpperInvariant(), url }, lastNavigationUrl = url
        });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var place = json.RootElement;
        Assert.AreEqual("Example Food Shop", place.GetProperty("name").GetString());
        Assert.AreEqual("Taipei City", place.GetProperty("address").GetString());
        Assert.AreEqual(25.0331, place.GetProperty("latitude").GetDouble());
        Assert.AreEqual(121.5655, place.GetProperty("longitude").GetDouble());
        Assert.AreEqual(420, place.GetProperty("distanceMeters").GetDouble());
        Assert.AreEqual(url, place.GetProperty("navigationUrl").GetString());
        Assert.AreEqual("google", place.GetProperty("provider").GetString());
        Assert.AreEqual(System.Text.Json.JsonValueKind.Null, place.GetProperty("rating").ValueKind);
        CollectionAssert.AreEqual(new[] { url }, place.GetProperty("resetNavigationUrls").EnumerateArray().Select(key => key.GetString()).ToArray());
        Assert.AreEqual(1, factory.ProviderCalls);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(",\"excludedNavigationUrls\":null,\"lastNavigationUrl\":null")]
    [DataRow(",\"excludedNavigationUrls\":[]")]
    public async Task Omitted_null_or_empty_history_preserves_top_level_place(string history)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent("{\"latitude\":25.033,\"longitude\":121.5654" + history + "}", System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual("Example Food Shop", json.RootElement.GetProperty("name").GetString());
        Assert.AreEqual(0, json.RootElement.GetProperty("resetNavigationUrls").GetArrayLength());
        Assert.AreEqual(1, factory.ProviderCalls);
    }

    [TestMethod]
    [DataRow("\"excludedNavigationUrls\":\"A\"")]
    [DataRow("\"excludedNavigationUrls\":{}")]
    [DataRow("\"excludedNavigationUrls\":true")]
    [DataRow("\"excludedNavigationUrls\":123")]
    [DataRow("\"excludedNavigationUrls\":[null]")]
    [DataRow("\"excludedNavigationUrls\":[123]")]
    [DataRow("\"excludedNavigationUrls\":[\" \"]")]
    [DataRow("\"excludedNavigationUrls\":[\"\"]")]
    [DataRow("\"lastNavigationUrl\":\" \"")]
    [DataRow("\"lastNavigationUrl\":\"\"")]
    [DataRow("\"lastNavigationUrl\":123")]
    [DataRow("\"lastNavigationUrl\":[]")]
    [DataRow("\"lastNavigationUrl\":{}")]
    [DataRow("\"lastNavigationUrl\":false")]
    public async Task Invalid_history_returns_invalid_request_without_provider(string history)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent("{\"latitude\":25.033,\"longitude\":121.5654," + history + "}", System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("invalid_request", (await response.Content.ReadFromJsonAsync<ApiErrorResponse>())?.ErrorCode);
        Assert.AreEqual(0, factory.ProviderCalls);
    }

    [TestMethod]
    [DataRow(1000, 2048, true)]
    [DataRow(1001, 2048, false)]
    [DataRow(1, 2049, false)]
    public async Task History_count_and_key_length_bounds_are_enforced(int count, int length, bool valid)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick", new {
            latitude = 25.033, longitude = 121.5654,
            excludedNavigationUrls = Enumerable.Repeat(new string('a', length), count).ToArray()
        });
        Assert.AreEqual(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(valid ? 1 : 0, factory.ProviderCalls);
        if (!valid) Assert.AreEqual("invalid_request", (await response.Content.ReadFromJsonAsync<ApiErrorResponse>())?.ErrorCode);
    }

    [TestMethod]
    [DataRow(2048, true)]
    [DataRow(2049, false)]
    public async Task Last_history_key_length_bounds_are_enforced(int length, bool valid)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick", new {
            latitude = 25.033, longitude = 121.5654, lastNavigationUrl = new string('a', length)
        });
        Assert.AreEqual(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(valid ? 1 : 0, factory.ProviderCalls);
        if (!valid) Assert.AreEqual("invalid_request", (await response.Content.ReadFromJsonAsync<ApiErrorResponse>())?.ErrorCode);
    }

    [TestMethod]
    [DataRow("", null)]
    [DataRow(",\"restaurantCategory\":null", null)]
    [DataRow(",\"restaurantCategory\":\"taiwanese-chinese\"", EatThis.Api.Domain.RestaurantCategory.TaiwaneseChinese)]
    [DataRow(",\"restaurantCategory\":\"japanese\"", EatThis.Api.Domain.RestaurantCategory.Japanese)]
    [DataRow(",\"restaurantCategory\":\"korean\"", EatThis.Api.Domain.RestaurantCategory.Korean)]
    [DataRow(",\"restaurantCategory\":\"hot-pot\"", EatThis.Api.Domain.RestaurantCategory.HotPot)]
    [DataRow(",\"restaurantCategory\":\"barbecue\"", EatThis.Api.Domain.RestaurantCategory.Barbecue)]
    [DataRow(",\"restaurantCategory\":\"italian\"", EatThis.Api.Domain.RestaurantCategory.Italian)]
    [DataRow(",\"restaurantCategory\":\"breakfast-brunch\"", EatThis.Api.Domain.RestaurantCategory.BreakfastBrunch)]
    [DataRow(",\"restaurantCategory\":\"fast-food\"", EatThis.Api.Domain.RestaurantCategory.FastFood)]
    [DataRow(",\"restaurantCategory\":\"vegetarian\"", EatThis.Api.Domain.RestaurantCategory.Vegetarian)]
    [DataRow(",\"restaurantCategory\":\"cafe-dessert\"", EatThis.Api.Domain.RestaurantCategory.CafeDessert)]
    public async Task Valid_category_reaches_provider_and_preserves_response_shape(string categoryJson, EatThis.Api.Domain.RestaurantCategory? expected)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent(
            "{\"latitude\":25.033,\"longitude\":121.5654,\"radiusMeters\":1000" + categoryJson + "}",
            System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(expected, factory.LastQuery?.RestaurantCategory);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        CollectionAssert.AreEquivalent(
            new[] { "name", "address", "latitude", "longitude", "distanceMeters", "navigationUrl", "provider", "rating", "resetNavigationUrls", "userRatingCount" },
            json.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
    }

    [TestMethod]
    [DataRow("\"\"")]
    [DataRow("\"all\"")]
    [DataRow("\"Japanese\"")]
    [DataRow("\" japanese \"")]
    [DataRow("\"restaurant\"")]
    [DataRow("\"pizza_restaurant\"")]
    [DataRow("123")]
    [DataRow("true")]
    [DataRow("[]")]
    [DataRow("{}")]
    public async Task Invalid_category_returns_stable_error_without_provider_call(string categoryJson)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent(
            "{\"latitude\":25.033,\"longitude\":121.5654,\"radiusMeters\":1000,\"restaurantCategory\":" + categoryJson + "}",
            System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.AreEqual("invalid_request", error?.ErrorCode);
        Assert.IsTrue(error?.Message.Contains("餐廳類型", StringComparison.Ordinal));
        Assert.AreEqual(0, factory.ProviderCalls);
    }

    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void Nonfinite_minimum_fails_domain_validation(double minimum)
    {
        Assert.IsFalse(EatThis.Api.Domain.NearbySearchQuery.TryCreate(
            new NearbyFoodRequest(25.033, 121.5654, 700, minimum), out _));
    }
    [TestMethod]
    [DataRow(0.5)]
    [DataRow(1.0)]
    [DataRow(1.5)]
    [DataRow(2.0)]
    [DataRow(2.5)]
    [DataRow(3.0)]
    [DataRow(3.5)]
    [DataRow(4.0)]
    [DataRow(4.5)]
    [DataRow(5.0)]
    public async Task Every_half_star_threshold_is_forwarded_once(double minRating)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick",
            new { latitude = 25.033, longitude = 121.5654, radiusMeters = 700, minRating });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.AreEqual("no_results", error?.ErrorCode);
        Assert.AreEqual("本次未找到符合搜尋條件的餐飲地點。", error?.Message);
        Assert.AreEqual(0, error?.RetryAfterSeconds);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(minRating, factory.LastQuery?.MinRating);
    }

    [TestMethod]
    public async Task Explicit_null_minimum_remains_unrestricted()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick",
            new { latitude = 25.033, longitude = 121.5654, radiusMeters = 700, minRating = (double?)null });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNull(factory.LastQuery?.MinRating);
        Assert.AreEqual(1, factory.ProviderCalls);
    }
    [TestMethod]
    [DataRow("0")]
    [DataRow("-0.5")]
    [DataRow("5.5")]
    [DataRow("4.3")]
    [DataRow("\"4.5\"")]
    [DataRow("true")]
    [DataRow("[]")]
    [DataRow("{}")]
    [DataRow("1e400")]
    [DataRow("NaN")]
    public async Task Invalid_rating_returns_stable_error_without_provider_call(string rating)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent(
            "{\"latitude\":25.033,\"longitude\":121.5654,\"radiusMeters\":700,\"minRating\":" + rating + "}",
            System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.AreEqual("invalid_request", error?.ErrorCode);
        Assert.AreEqual(0, factory.ProviderCalls);
    }

    [TestMethod]
    [DataRow("{", "application/json")]
    [DataRow("null", "application/json")]
    [DataRow("", "application/json")]
    [DataRow("{}", "text/plain")]
    public async Task Invalid_body_returns_stable_error_without_provider_call(string payload, string contentType)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var body = new StringContent(payload, System.Text.Encoding.UTF8, contentType);
        using var response = await client.PostAsync("/api/nearby-food/pick", body);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.AreEqual("invalid_request", error?.ErrorCode);
        Assert.AreEqual(0, factory.ProviderCalls);
    }

    [TestMethod]
    public async Task Unrestricted_response_includes_nullable_rating()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick",
            new { latitude = 25.033, longitude = 121.5654, radiusMeters = 700 });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(System.Text.Json.JsonValueKind.Null, json.RootElement.GetProperty("rating").ValueKind);
    }
    [TestMethod]
    public async Task Valid_request_returns_one_selected_place()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var selected = await response.Content.ReadFromJsonAsync<SelectedPlaceResponse>();
        Assert.IsNotNull(selected);
        Assert.AreEqual("Example Food Shop", selected.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(selected.NavigationUrl));
    }

    [TestMethod]
    public async Task Local_web_origin_is_allowed_for_api_requests()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/nearby-food/pick");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        using var response = await client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.AreEqual(
            "http://localhost:5173",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [TestMethod]
    public async Task Omitted_radius_uses_the_three_kilometer_default()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654 });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3000, factory.LastQuery?.RadiusMeters);
    }

    [TestMethod]
    [DataRow(100)]
    [DataRow(3000)]
    public async Task Radius_at_public_bounds_is_accepted_and_calls_provider_once(int radiusMeters)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters });

        Assert.AreNotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(radiusMeters, factory.LastQuery?.RadiusMeters);
    }

    [TestMethod]
    public async Task Caller_language_option_is_ignored_and_provider_query_remains_neutral()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new
            {
                latitude = 25.0330,
                longitude = 121.5654,
                radiusMeters = 700,
                languageCode = "en-US",
            });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(700, factory.LastQuery?.RadiusMeters);
        Assert.IsNull(typeof(NearbyFoodRequest).GetProperty("LanguageCode"));
        Assert.IsNull(typeof(EatThis.Api.Domain.NearbySearchQuery).GetProperty("LanguageCode"));
    }

    [TestMethod]
    [DataRow(91.0, 121.5654, 3000)]
    [DataRow(25.0330, 181.0, 3000)]
    [DataRow(25.0330, 121.5654, 99)]
    [DataRow(25.0330, 121.5654, 3001)]
    [DataRow(25.0330, 121.5654, 5000)]
    public async Task Invalid_location_or_radius_returns_bad_request(
        double latitude,
        double longitude,
        int radiusMeters)
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude, longitude, radiusMeters });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.IsNotNull(error);
        Assert.AreEqual("invalid_request", error.ErrorCode);
        Assert.AreEqual(0, factory.ProviderCalls);
    }

    private sealed record SelectedPlaceResponse(
        string Name,
        string Address,
        double Latitude,
        double Longitude,
        double DistanceMeters,
        string NavigationUrl,
        string Provider);
}
