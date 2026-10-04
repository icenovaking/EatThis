using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.Extensions.Options;

namespace EatThis.Api.Infrastructure;

public sealed class GooglePlacesProvider(
    HttpClient httpClient,
    IOptions<GooglePlacesOptions> options,
    ILogger<GooglePlacesProvider> logger) : IPlaceProvider
{
    public static readonly Uri DefaultBaseAddress = new("https://places.googleapis.com/");
    public const int DefaultTimeoutSeconds = 10;

    private const string FieldMask =
        "places.displayName,places.formattedAddress,places.location,places.googleMapsUri,places.rating,places.userRatingCount";
    private const string LanguageCode = "zh-TW";

    private static readonly string[] IncludedTypes =
    ["restaurant", "cafe", "fast_food_restaurant", "food_court", "bakery", "meal_takeaway"];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
        NearbySearchQuery query,
        CancellationToken cancellationToken)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("Google Places API key is not configured.");
            throw new PlaceProviderException(PlaceProviderFailureKind.Configuration);
        }

        var request = new GoogleNearbySearchRequest(
            GetIncludedTypes(query.RestaurantCategory),
            20,
            LanguageCode,
            new GoogleLocationRestriction(
                new GoogleCircle(
                    new GoogleLatLng(query.Latitude, query.Longitude),
                    query.RadiusMeters)));

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/places:searchNearby")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        httpRequest.Headers.TryAddWithoutValidation("X-Goog-Api-Key", apiKey);
        httpRequest.Headers.TryAddWithoutValidation("X-Goog-FieldMask", FieldMask);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Google Places request timed out.");
            throw new PlaceProviderException(PlaceProviderFailureKind.Unavailable);
        }
        catch (HttpRequestException)
        {
            logger.LogWarning("Google Places transport request failed.");
            throw new PlaceProviderException(PlaceProviderFailureKind.Unavailable);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                logger.LogWarning(
                    "Google Places rejected the request with status code {StatusCode}.",
                    (int)response.StatusCode);
                throw new PlaceProviderException(PlaceProviderFailureKind.Authentication);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("Google Places quota was exceeded.");
                throw new PlaceProviderException(PlaceProviderFailureKind.Quota);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Google Places returned status code {StatusCode}.",
                    (int)response.StatusCode);
                throw new PlaceProviderException(PlaceProviderFailureKind.Unavailable);
            }

            GoogleNearbySearchResponse? payload;
            try
            {
                payload = await response.Content.ReadFromJsonAsync<GoogleNearbySearchResponse>(
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException)
            {
                logger.LogWarning("Google Places returned an invalid JSON response.");
                throw new PlaceProviderException(PlaceProviderFailureKind.Unavailable);
            }

            return payload?.Places is null
                ? []
                : payload.Places
                    .Select(place => MapCandidate(place, query))
                    .Where(candidate => candidate is not null)
                    .Select(candidate => candidate!)
                    .ToArray();
        }
    }

    private static IReadOnlyList<string> GetIncludedTypes(RestaurantCategory? category) => category switch
    {
        null => IncludedTypes,
        RestaurantCategory.TaiwaneseChinese => ["taiwanese_restaurant", "chinese_restaurant"],
        RestaurantCategory.Japanese => ["japanese_restaurant", "sushi_restaurant", "ramen_restaurant"],
        RestaurantCategory.Korean => ["korean_restaurant", "korean_barbecue_restaurant"],
        RestaurantCategory.HotPot => ["hot_pot_restaurant"],
        RestaurantCategory.Barbecue => ["barbecue_restaurant", "yakiniku_restaurant"],
        RestaurantCategory.Italian => ["italian_restaurant", "pizza_restaurant"],
        RestaurantCategory.BreakfastBrunch => ["breakfast_restaurant", "brunch_restaurant"],
        RestaurantCategory.FastFood => ["fast_food_restaurant", "hamburger_restaurant"],
        RestaurantCategory.Vegetarian => ["vegetarian_restaurant", "vegan_restaurant"],
        RestaurantCategory.CafeDessert => ["cafe", "coffee_shop", "dessert_shop", "dessert_restaurant"],
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };

    private static PlaceCandidate? MapCandidate(GooglePlace place, NearbySearchQuery query)
    {
        if (place.DisplayName?.Text is not { Length: > 0 } name ||
            place.Location is null ||
            string.IsNullOrWhiteSpace(place.GoogleMapsUri))
        {
            return null;
        }

        var distanceMeters = DistanceCalculator.MetersBetween(
            query.Latitude,
            query.Longitude,
            place.Location.Latitude,
            place.Location.Longitude);

        if (!double.IsFinite(distanceMeters) || distanceMeters > query.RadiusMeters)
        {
            return null;
        }

        return new PlaceCandidate(
            name.Trim(),
            place.FormattedAddress?.Trim() ?? string.Empty,
            place.Location.Latitude,
            place.Location.Longitude,
            Math.Round(distanceMeters, 1),
            place.GoogleMapsUri,
            "google",
            place.Rating is double rating && double.IsFinite(rating) && rating is >= 1 and <= 5 ? rating : null,
            place.UserRatingCount.ValueKind == JsonValueKind.Number &&
            place.UserRatingCount.TryGetInt32(out var count) && count >= 0 ? count : null);
    }
}

internal static class DistanceCalculator
{
    private const double EarthRadiusMeters = 6_371_000;

    public static double MetersBetween(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        var latitudeDelta = DegreesToRadians(latitudeB - latitudeA);
        var longitudeDelta = DegreesToRadians(longitudeB - longitudeA);
        var a = Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
                Math.Cos(DegreesToRadians(latitudeA)) *
                Math.Cos(DegreesToRadians(latitudeB)) *
                Math.Pow(Math.Sin(longitudeDelta / 2), 2);
        return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}

internal sealed record GoogleNearbySearchRequest(
    [property: JsonPropertyName("includedTypes")] IReadOnlyList<string> IncludedTypes,
    [property: JsonPropertyName("maxResultCount")] int MaxResultCount,
    [property: JsonPropertyName("languageCode")] string LanguageCode,
    [property: JsonPropertyName("locationRestriction")] GoogleLocationRestriction LocationRestriction);

internal sealed record GoogleLocationRestriction(
    [property: JsonPropertyName("circle")] GoogleCircle Circle);

internal sealed record GoogleCircle(
    [property: JsonPropertyName("center")] GoogleLatLng Center,
    [property: JsonPropertyName("radius")] double Radius);

internal sealed record GoogleLatLng(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude);

internal sealed record GoogleNearbySearchResponse(
    [property: JsonPropertyName("places")] IReadOnlyList<GooglePlace>? Places);

internal sealed record GooglePlace(
    [property: JsonPropertyName("displayName")] GoogleDisplayName? DisplayName,
    [property: JsonPropertyName("formattedAddress")] string? FormattedAddress,
    [property: JsonPropertyName("location")] GoogleLatLng? Location,
    [property: JsonPropertyName("googleMapsUri")] string? GoogleMapsUri,
    [property: JsonPropertyName("rating")] double? Rating,
    [property: JsonPropertyName("userRatingCount")] JsonElement UserRatingCount);

internal sealed record GoogleDisplayName(
    [property: JsonPropertyName("text")] string? Text);
