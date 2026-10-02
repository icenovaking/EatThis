using EatThis.Api.Contracts;

namespace EatThis.Api.Domain;

public readonly record struct NearbySearchQuery(
    double Latitude,
    double Longitude,
    int RadiusMeters,
    double? MinRating = null,
    RestaurantCategory? RestaurantCategory = null)
{
    public const int DefaultRadiusMeters = 3000;
    public const int MinimumRadiusMeters = 100;
    public const int MaximumRadiusMeters = 3000;

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

        if (request.MinRating is double minimum &&
            (!double.IsFinite(minimum) || minimum is < 0.5 or > 5 || minimum * 2 != Math.Truncate(minimum * 2)))
        {
            return false;
        }

        if (!RestaurantCategoryParser.TryParse(request.RestaurantCategory, out var category))
        {
            return false;
        }

        query = new NearbySearchQuery(request.Latitude, request.Longitude, radiusMeters, request.MinRating, category);
        return true;
    }
}
