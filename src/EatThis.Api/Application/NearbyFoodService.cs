using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using EatThis.Api.Infrastructure;

namespace EatThis.Api.Application;

public sealed class NearbyFoodService(
    IPlaceProvider placeProvider,
    IRandomSource randomSource)
{
    public async Task<PickResult> PickAsync(
        NearbyFoodRequest? request,
        CancellationToken cancellationToken)
    {
        if (!NearbySearchQuery.TryCreate(request, out var query))
        {
            return new PickResult.InvalidRequest();
        }

        IReadOnlyList<PlaceCandidate> candidates;
        try
        {
            candidates = await placeProvider.SearchAsync(query, cancellationToken);
        }
        catch (PlaceProviderException exception)
        {
            return new PickResult.ProviderFailure(exception.StatusCode);
        }

        var usableCandidates = candidates
            .Where(candidate => PlaceCandidateRules.IsUsable(candidate, query))
            .DistinctBy(candidate => candidate.NavigationUrl, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (usableCandidates.Length == 0)
        {
            return new PickResult.NoResults();
        }

        var index = randomSource.Next(usableCandidates.Length);
        if (index < 0 || index >= usableCandidates.Length)
        {
            throw new InvalidOperationException("The random source returned an invalid candidate index.");
        }

        return new PickResult.Success(usableCandidates[index]);
    }
}

public abstract record PickResult
{
    private PickResult()
    {
    }

    public sealed record Success(PlaceCandidate Place) : PickResult;

    public sealed record InvalidRequest : PickResult;

    public sealed record NoResults : PickResult;

    public sealed record ProviderFailure(int StatusCode) : PickResult;
}
