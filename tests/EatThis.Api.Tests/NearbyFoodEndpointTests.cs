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
