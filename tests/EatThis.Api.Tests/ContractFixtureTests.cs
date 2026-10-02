using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class ContractFixtureTests
{
    [TestMethod]
    public async Task Rated_contract_fixture_qualifies_at_inclusive_threshold()
    {
        var fixture = LoadFixture<PlaceCandidate>("nearby-food-success.json");
        await using var factory = new ConfigurableApiFactory(new FixtureProvider(fixture));
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick",
            new { latitude = 25.033, longitude = 121.5654, radiusMeters = 700, minRating = 4 });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(fixture, await response.Content.ReadFromJsonAsync<PlaceCandidate>());
    }
    [TestMethod]
    public async Task Api_success_response_matches_the_shared_frontend_contract_fixture()
    {
        var fixture = LoadFixture<PlaceCandidate>("nearby-food-success.json");
        await using var factory = new ConfigurableApiFactory(new FixtureProvider(fixture));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var actual = await response.Content.ReadFromJsonAsync<PlaceCandidate>();
        Assert.IsNotNull(actual);
        Assert.AreEqual(fixture, actual);
        Assert.AreEqual(4.3, actual.Rating);
        var publicJson = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(publicJson.Contains("apiKey", StringComparison.OrdinalIgnoreCase));
    }

    private static T LoadFixture<T>(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "EatThis.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.IsNotNull(directory);
        var path = Path.Combine(directory!.FullName, "tests", "fixtures", fileName);
        return JsonSerializer.Deserialize<T>(
                File.ReadAllText(path),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!
            ?? throw new InvalidOperationException($"Fixture {fileName} is empty.");
    }

    private sealed class FixtureProvider(PlaceCandidate candidate) : IPlaceProvider
    {
        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PlaceCandidate>>([candidate]);
    }
}
