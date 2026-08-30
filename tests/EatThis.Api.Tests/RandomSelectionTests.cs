using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class RandomSelectionTests
{
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
