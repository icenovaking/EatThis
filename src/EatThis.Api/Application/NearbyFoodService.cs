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
            .Where(candidate => query.MinRating is null || candidate.Rating >= query.MinRating)
            .DistinctBy(candidate => candidate.NavigationUrl, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (usableCandidates.Length == 0)
        {
            return new PickResult.NoResults();
        }

        var excluded = new HashSet<string>(request!.ExcludedNavigationUrls ?? [], StringComparer.OrdinalIgnoreCase);
        var drawCandidates = usableCandidates.Where(candidate => !excluded.Contains(candidate.NavigationUrl)).ToArray();
        string[] resetNavigationUrls = [];
        if (drawCandidates.Length == 0)
        {
            resetNavigationUrls = usableCandidates.Select(candidate => candidate.NavigationUrl).ToArray();
            drawCandidates = usableCandidates.Length == 1
                ? usableCandidates
                : usableCandidates.Where(candidate => !string.Equals(
                    candidate.NavigationUrl, request.LastNavigationUrl, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        var index = randomSource.Next(drawCandidates.Length);
        if (index < 0 || index >= drawCandidates.Length)
        {
            throw new InvalidOperationException("The random source returned an invalid candidate index.");
        }

        return new PickResult.Success(drawCandidates[index], resetNavigationUrls);
    }
}

public abstract record PickResult
{
    private PickResult()
    {
    }

    public sealed record Success(PlaceCandidate Place, IReadOnlyList<string> ResetNavigationUrls) : PickResult;

    public sealed record InvalidRequest : PickResult;

    public sealed record NoResults : PickResult;

    public sealed record ProviderFailure(int StatusCode) : PickResult;
}
