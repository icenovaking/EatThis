using System.Net;
using System.Net.Http.Json;
using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using EatThis.Api.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class FailureResponseTests
{
    [TestMethod]
    public async Task Empty_normalized_candidates_return_stable_no_results_response()
    {
        await using var factory = new ConfigurableApiFactory(new EmptyProvider());
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.IsNotNull(error);
        Assert.AreEqual("no_results", error.ErrorCode);
        Assert.IsFalse((await response.Content.ReadAsStringAsync()).Contains("Google", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Provider_authentication_failure_returns_gateway_error_without_upstream_body()
    {
        await using var factory = new ConfigurableApiFactory(
            new FailingProvider(PlaceProviderFailureKind.Authentication));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.IsNotNull(error);
        Assert.AreEqual("provider_unavailable", error.ErrorCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(body.Contains("upstream-secret", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("X-Goog-Api-Key", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Provider_outage_returns_service_unavailable_error()
    {
        await using var factory = new ConfigurableApiFactory(
            new FailingProvider(PlaceProviderFailureKind.Unavailable));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.IsNotNull(error);
        Assert.AreEqual("provider_unavailable", error.ErrorCode);
    }

    private sealed class EmptyProvider : IPlaceProvider
    {
        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PlaceCandidate>>([]);
    }

    private sealed class FailingProvider(PlaceProviderFailureKind kind) : IPlaceProvider
    {
        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken) =>
            Task.FromException<IReadOnlyList<PlaceCandidate>>(new PlaceProviderException(kind));
    }
}
