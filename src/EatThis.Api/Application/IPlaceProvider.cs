using EatThis.Api.Contracts;
using EatThis.Api.Domain;

namespace EatThis.Api.Application;

public interface IPlaceProvider
{
    Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
        NearbySearchQuery query,
        CancellationToken cancellationToken);
}
