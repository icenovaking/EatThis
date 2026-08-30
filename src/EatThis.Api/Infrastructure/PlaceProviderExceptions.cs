namespace EatThis.Api.Infrastructure;

public enum PlaceProviderFailureKind
{
    Configuration,
    Authentication,
    Quota,
    Unavailable,
}

public sealed class PlaceProviderException : Exception
{
    public PlaceProviderException(PlaceProviderFailureKind kind)
        : base("The place provider could not complete the request.")
    {
        StatusCode = kind is PlaceProviderFailureKind.Authentication or PlaceProviderFailureKind.Quota
            ? StatusCodes.Status502BadGateway
            : StatusCodes.Status503ServiceUnavailable;
    }

    public int StatusCode { get; }
}
