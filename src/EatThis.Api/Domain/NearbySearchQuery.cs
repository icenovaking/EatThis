using EatThis.Api.Contracts;

namespace EatThis.Api.Domain;

public readonly record struct NearbySearchQuery(
    double Latitude,
    double Longitude,
    int RadiusMeters)
{
    public const int DefaultRadiusMeters = 3000;
    public const int MinimumRadiusMeters = 100;
    public const int MaximumRadiusMeters = 5000;

    public static bool TryCreate(NearbyFoodRequest? request, out NearbySearchQuery query)
    {
        query = default;
        if (request is null ||
            !double.IsFinite(request.Latitude) ||
            request.Latitude is < -90 or > 90 ||
            !double.IsFinite(request.Longitude) ||
            request.Longitude is < -180 or > 180)
        {
            return false;
        }

        var radiusMeters = request.RadiusMeters ?? DefaultRadiusMeters;
        if (radiusMeters is < MinimumRadiusMeters or > MaximumRadiusMeters)
        {
            return false;
        }

        query = new NearbySearchQuery(request.Latitude, request.Longitude, radiusMeters);
        return true;
    }
}
