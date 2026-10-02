using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class ProviderNeutralContractTests
{
    [TestMethod]
    public async Task Alternative_provider_missing_rating_is_excluded_by_threshold()
    {
        var service = new NearbyFoodService(new OSMLikeProvider(), new UnexpectedRandomSource());
        var result = await service.PickAsync(new NearbyFoodRequest(25.033, 121.5654, 700, 4), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.NoResults>(result);
    }

    private sealed class UnexpectedRandomSource : IRandomSource
    {
        public int Next(int maxExclusive) => throw new AssertFailedException("No eligible candidates must not invoke random source.");
    }
    [TestMethod]
    public async Task Alternative_provider_can_supply_the_same_selection_contract()
    {
        var provider = new OSMLikeProvider();
        var service = new NearbyFoodService(provider, new FirstCandidateRandomSource());

        var result = await service.PickAsync(
            new NearbyFoodRequest(25.0330, 121.5654),
            CancellationToken.None);

        var success = result as PickResult.Success;
        Assert.IsNotNull(success);
        Assert.AreEqual("osm", success!.Place.Provider);
        Assert.AreEqual("OSM Food Stall", success.Place.Name);
        Assert.AreEqual("https://www.openstreetmap.org/node/123", success.Place.NavigationUrl);
        Assert.IsNull(success.Place.Rating);
    }

    private sealed class OSMLikeProvider : IPlaceProvider
    {
        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<PlaceCandidate> candidates =
            [
                new(
                    "OSM Food Stall",
                    "Taipei City",
                    query.Latitude,
                    query.Longitude,
                    10,
                    "https://www.openstreetmap.org/node/123",
                    "osm"),
            ];
            return Task.FromResult(candidates);
        }
    }

    private sealed class FirstCandidateRandomSource : IRandomSource
    {
        public int Next(int maxExclusive) => 0;
    }
}
