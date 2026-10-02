using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class CandidateNormalizationTests
{
    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(0.0)]
    [DataRow(0.5)]
    [DataRow(5.1)]
    public void Alternative_provider_invalid_rating_is_unusable(double rating)
    {
        var candidate = new PlaceCandidate("Food", "Taipei", 25.033, 121.5654, 10, "https://example.com/food", "alternative", rating);
        Assert.IsFalse(PlaceCandidateRules.IsUsable(candidate, new NearbySearchQuery(25.033, 121.5654, 700)));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(1.0)]
    [DataRow(4.3)]
    [DataRow(5.0)]
    public void Alternative_provider_valid_or_missing_rating_is_usable(double? rating)
    {
        var candidate = new PlaceCandidate("Food", "Taipei", 25.033, 121.5654, 10, "https://example.com/food", "alternative", rating);
        Assert.IsTrue(PlaceCandidateRules.IsUsable(candidate, new NearbySearchQuery(25.033, 121.5654, 700)));
    }
    [TestMethod]
    public async Task Selection_filters_unusable_and_duplicate_candidates_before_picking()
    {
        var provider = new NoisyProvider();
        var service = new NearbyFoodService(provider, new SecondCandidateRandomSource());

        var result = await service.PickAsync(
            new NearbyFoodRequest(25.0330, 121.5654, 3000),
            CancellationToken.None);

        var success = result as PickResult.Success;
        Assert.IsNotNull(success);
        Assert.AreEqual("Valid B", success!.Place.Name);
        Assert.AreEqual(1, provider.CallCount);
    }

    private sealed class NoisyProvider : IPlaceProvider
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken)
        {
            CallCount++;
            IReadOnlyList<PlaceCandidate> candidates =
            [
                new("", "No name", 25.0331, 121.5655, 10, "https://example.com/no-name", "google"),
                new("Bad coordinates", "Taipei", 91, 121.5655, 10, "https://example.com/bad-coordinates", "google"),
                new("HTTP navigation", "Taipei", 25.0331, 121.5655, 10, "http://example.com/http", "google"),
                new("Outside radius", "Taipei", 25.0331, 121.5655, 3001, "https://example.com/outside", "google"),
                new("Valid A", "Taipei", 25.0331, 121.5655, 10, "https://example.com/a", "google"),
                new("Duplicate A", "Taipei", 25.0331, 121.5655, 10, "https://example.com/a", "google"),
                new("Valid B", "Taipei", 25.0332, 121.5656, 20, "https://example.com/b", "osm"),
            ];
            return Task.FromResult(candidates);
        }
    }

    private sealed class SecondCandidateRandomSource : IRandomSource
    {
        public int Next(int maxExclusive) => 1;
    }
}
