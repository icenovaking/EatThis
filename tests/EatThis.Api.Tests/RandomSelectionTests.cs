using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class RandomSelectionTests
{
    private static string Url(string name) => $"https://example.com/{name}";
    private static NearbyFoodRequest HistoryRequest(string[] excluded, string? last = null, double? minRating = null) =>
        new(25.033, 121.5654, 700, minRating, null, excluded, last);

    [TestMethod]
    [DataRow("C")]
    [DataRow("E")]
    public async Task Unseen_candidate_has_priority_across_search_conditions(string unseen)
    {
        var provider = new CandidateProvider([Candidate("A", 10), Candidate("B", 20), Candidate(unseen, 30)]);
        var random = new RecordingRandomSource(0);
        var result = await new NearbyFoodService(provider, random).PickAsync(
            HistoryRequest([Url("A").ToUpperInvariant(), Url("B"), Url("D")]), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual(unseen, success.Place.Name);
        Assert.AreEqual(1, random.LastMaxExclusive);
        Assert.AreEqual(0, success.ResetNavigationUrls.Count);
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    [DataRow(0, "A")]
    [DataRow(1, "C")]
    public async Task Exhausted_round_after_A_C_B_excludes_last_B(int index, string expected)
    {
        var provider = new CandidateProvider([Candidate("A", 10), Candidate("B", 20), Candidate("C", 30)]);
        var random = new RecordingRandomSource(index);
        var result = await new NearbyFoodService(provider, random).PickAsync(
            HistoryRequest([Url("A"), Url("C"), Url("B"), Url("D")], Url("B").ToUpperInvariant()), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual(expected, success.Place.Name);
        Assert.AreEqual(2, random.LastMaxExclusive);
        CollectionAssert.AreEquivalent(new[] { Url("A"), Url("B"), Url("C") }, success.ResetNavigationUrls.ToArray());
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    public async Task Second_round_draws_A_and_B_once_after_C()
    {
        var provider = new CandidateProvider([Candidate("A", 10), Candidate("B", 20), Candidate("C", 30)]);
        var service = new NearbyFoodService(provider, new RecordingRandomSource(0));
        var first = await service.PickAsync(HistoryRequest([Url("C")], Url("C")), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(first, out var a);
        Assert.AreEqual("A", a.Place.Name);
        Assert.AreEqual(0, a.ResetNavigationUrls.Count);
        var second = await service.PickAsync(HistoryRequest([Url("C"), Url("A")], Url("A")), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(second, out var b);
        Assert.AreEqual("B", b.Place.Name);
        Assert.AreEqual(0, b.ResetNavigationUrls.Count);
        Assert.AreEqual(2, provider.CallCount);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("https://example.com/D")]
    public async Task Exhausted_round_without_matching_last_uses_all_current_candidates(string? last)
    {
        var provider = new CandidateProvider([Candidate("A", 10), Candidate("B", 20)]);
        var random = new RecordingRandomSource(1);
        var result = await new NearbyFoodService(provider, random).PickAsync(
            HistoryRequest([Url("A"), Url("B"), Url("D")], last), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual("B", success.Place.Name);
        Assert.AreEqual(2, random.LastMaxExclusive);
        CollectionAssert.AreEquivalent(new[] { Url("A"), Url("B") }, success.ResetNavigationUrls.ToArray());
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    public async Task Singleton_can_repeat_and_reset_after_deduplication_and_filters()
    {
        var provider = new CandidateProvider([
            RatedCandidate("low", 3.9), RatedCandidate("outside", 5) with { DistanceMeters = 701 },
            RatedCandidate("A", 4), RatedCandidate("duplicate", 4, Url("A").ToUpperInvariant())]);
        var random = new RecordingRandomSource(0);
        var result = await new NearbyFoodService(provider, random).PickAsync(
            HistoryRequest([Url("A")], Url("A"), 4), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.Success>(result, out var success);
        Assert.AreEqual("A", success.Place.Name);
        Assert.AreEqual(1, random.LastMaxExclusive);
        CollectionAssert.AreEqual(new[] { Url("A") }, success.ResetNavigationUrls.ToArray());
        Assert.AreEqual(1, provider.CallCount);
    }

    [TestMethod]
    public async Task History_does_not_relax_empty_candidate_set_or_draw()
    {
        var provider = new CandidateProvider([RatedCandidate("A", 3.9)]);
        var random = new RecordingRandomSource(0);
        var result = await new NearbyFoodService(provider, random).PickAsync(
            HistoryRequest([Url("A")], Url("A"), 4), CancellationToken.None);
        Assert.IsInstanceOfType<PickResult.NoResults>(result);
        Assert.IsNull(random.LastMaxExclusive);
        Assert.AreEqual(1, provider.CallCount);
    }

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
