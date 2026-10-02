using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class RandomSelectionTests
{
    private static NearbyFoodRequest RatedRequest(double? minimum) =>
        new(25.033, 121.5654, 700, minimum);

    private static PlaceCandidate RatedCandidate(string name, double? rating, string? url = null) =>
        new(name, "Taipei", 25.033, 121.5654, 10, url ?? $"https://example.com/{name}", "alternative", rating);

    [TestMethod]
    [DataRow(0, "B")]
    [DataRow(1, "C")]
    public async Task Minimum_rating_filters_inclusively_before_random(int index, string expected)
    {
        var provider = new CandidateProvider([RatedCandidate("A", 3.9), RatedCandidate("B", 4), RatedCandidate("C", 4.3), RatedCandidate("D", null)]);
        var random = new RecordingRandomSource(index);
        var result = await new NearbyFoodService(provider, random).PickAsync(RatedRequest(4), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual(expected, success.Place.Name);
        Assert.AreEqual(2, random.LastMaxExclusive);
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    public async Task Unrestricted_selection_includes_missing_rating()
    {
        var provider = new CandidateProvider([RatedCandidate("A", 3.9), RatedCandidate("B", 4), RatedCandidate("C", 4.3), RatedCandidate("D", null)]);
        var random = new RecordingRandomSource(3);
        var result = await new NearbyFoodService(provider, random).PickAsync(RatedRequest(null), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual("D", success.Place.Name);
        Assert.AreEqual(4, random.LastMaxExclusive);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Rating_filter_precedes_navigation_deduplication(bool reverse)
    {
        var candidates = new[] { RatedCandidate("low", 3.9, "https://example.com/same"), RatedCandidate("eligible", 4.3, "https://example.com/same") };
        var provider = new CandidateProvider(reverse ? candidates.Reverse().ToArray() : candidates);
        var random = new RecordingRandomSource(0);
        var result = await new NearbyFoodService(provider, random).PickAsync(RatedRequest(4), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual("eligible", success.Place.Name);
        Assert.AreEqual(1, random.LastMaxExclusive);
    }

    [TestMethod]
    public async Task No_rating_qualified_candidates_never_randomize_or_retry()
    {
        var provider = new CandidateProvider([RatedCandidate("A", 3.9), RatedCandidate("B", 4), RatedCandidate("C", 4.3), RatedCandidate("D", null)]);
        var random = new RecordingRandomSource(0);
        var result = await new NearbyFoodService(provider, random).PickAsync(RatedRequest(4.5), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.NoResults>(result);
        Assert.IsNull(random.LastMaxExclusive);
        Assert.AreEqual(1, provider.CallCount);
    }
    [TestMethod]
    public async Task Multiple_provider_candidates_produce_exactly_one_selected_place()
    {
        var random = new RecordingRandomSource(2);
        var provider = new CandidateProvider(
        [
            Candidate("A", 10),
            Candidate("B", 20),
            Candidate("C", 30),
        ]);
        var service = new NearbyFoodService(provider, random);

        var result = await service.PickAsync(
            new NearbyFoodRequest(25.0330, 121.5654, 3000),
            CancellationToken.None);

        var success = result as PickResult.Success;
        Assert.IsNotNull(success);
        Assert.AreEqual("C", success!.Place.Name);
        Assert.AreEqual(3, random.LastMaxExclusive);
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    public async Task Empty_provider_candidates_produce_no_results()
    {
        var service = new NearbyFoodService(
            new CandidateProvider([]),
            new RecordingRandomSource(0));

        var result = await service.PickAsync(
            new NearbyFoodRequest(25.0330, 121.5654, 3000),
            CancellationToken.None);

        Assert.IsInstanceOfType<PickResult.NoResults>(result);
    }

    private static PlaceCandidate Candidate(string name, double distanceMeters) =>
        new(
            name,
            "Taipei City",
            25.0331,
            121.5655,
            distanceMeters,
            $"https://example.com/{name}",
            "google");

    private sealed class CandidateProvider(IReadOnlyList<PlaceCandidate> candidates) : IPlaceProvider
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(candidates);
        }
    }

    private sealed class RecordingRandomSource(int selectedIndex) : IRandomSource
    {
        public int? LastMaxExclusive { get; private set; }

        public int Next(int maxExclusive)
        {
            LastMaxExclusive = maxExclusive;
            return selectedIndex;
        }
    }
}
