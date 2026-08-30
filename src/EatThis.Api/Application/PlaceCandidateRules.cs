using EatThis.Api.Contracts;
using EatThis.Api.Domain;

namespace EatThis.Api.Application;

public static class PlaceCandidateRules
{
    public static bool IsUsable(PlaceCandidate candidate, NearbySearchQuery query)
    {
        return !string.IsNullOrWhiteSpace(candidate.Name) &&
               !string.IsNullOrWhiteSpace(candidate.Address) &&
               double.IsFinite(candidate.Latitude) &&
               candidate.Latitude is >= -90 and <= 90 &&
               double.IsFinite(candidate.Longitude) &&
               candidate.Longitude is >= -180 and <= 180 &&
               double.IsFinite(candidate.DistanceMeters) &&
               candidate.DistanceMeters >= 0 &&
               candidate.DistanceMeters <= query.RadiusMeters &&
               IsHttpsUrl(candidate.NavigationUrl) &&
               !string.IsNullOrWhiteSpace(candidate.Provider);
    }

    private static bool IsHttpsUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
