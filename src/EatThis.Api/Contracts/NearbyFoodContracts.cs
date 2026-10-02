using System.Text.Json.Serialization;

namespace EatThis.Api.Contracts;

public sealed record NearbyFoodRequest(
    double Latitude,
    double Longitude,
    int? RadiusMeters = null,
    [property: JsonNumberHandling(JsonNumberHandling.Strict)] double? MinRating = null);

public sealed record PlaceCandidate(
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    double DistanceMeters,
    string NavigationUrl,
    string Provider,
    double? Rating = null);

public sealed record ApiErrorResponse(
    [property: JsonPropertyName("errorCode")] string ErrorCode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("retryAfterSeconds")] int? RetryAfterSeconds = null)
{
    public static ApiErrorResponse InvalidRequest { get; } =
        new("invalid_request", "位置、搜尋範圍或最低評分不符合規定。");

    public static ApiErrorResponse NoResults { get; } =
        new("no_results", "本次未找到符合搜尋條件的餐飲地點。", 0);

    public static ApiErrorResponse ProviderUnavailable { get; } =
        new("provider_unavailable", "附近地點服務暫時無法使用，請稍後再試。", 30);

    public static ApiErrorResponse RateLimited { get; } =
        new("rate_limited", "請稍候再試。", 60);
}
