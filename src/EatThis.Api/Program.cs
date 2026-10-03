using System.Threading.RateLimiting;
using System.Text.Json;
using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173", "http://localhost:4173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.Configure<GooglePlacesOptions>(
    builder.Configuration.GetSection(GooglePlacesOptions.SectionName));
builder.Services.AddHttpClient<GooglePlacesProvider>(client =>
{
    client.BaseAddress = GooglePlacesProvider.DefaultBaseAddress;
    client.Timeout = TimeSpan.FromSeconds(GooglePlacesProvider.DefaultTimeoutSeconds);
});
builder.Services.AddSingleton<IPlaceProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<GooglePlacesProvider>());
builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddScoped<NearbyFoodService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("nearby-food", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiErrorResponse.RateLimited,
            cancellationToken);
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost(
        "/api/nearby-food/pick",
        async (HttpRequest httpRequest, NearbyFoodService service, CancellationToken cancellationToken) =>
        {
            NearbyFoodRequest? request;
            try
            {
                request = await httpRequest.ReadFromJsonAsync<NearbyFoodRequest>(cancellationToken);
            }
            catch (JsonException)
            {
                return Results.BadRequest(ApiErrorResponse.InvalidRequest);
            }
            catch (InvalidOperationException) when (!httpRequest.HasJsonContentType())
            {
                return Results.BadRequest(ApiErrorResponse.InvalidRequest);
            }
            var result = await service.PickAsync(request, cancellationToken);
            return result switch
            {
                PickResult.Success success => Results.Ok(new NearbyFoodResponse(
                    success.Place.Name, success.Place.Address, success.Place.Latitude, success.Place.Longitude,
                    success.Place.DistanceMeters, success.Place.NavigationUrl, success.Place.Provider,
                    success.Place.Rating, success.ResetNavigationUrls)),
                PickResult.InvalidRequest => Results.BadRequest(ApiErrorResponse.InvalidRequest),
                PickResult.NoResults => Results.NotFound(ApiErrorResponse.NoResults),
                PickResult.ProviderFailure failure => Results.Json(
                    ApiErrorResponse.ProviderUnavailable,
                    statusCode: failure.StatusCode),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
            };
        })
    .RequireRateLimiting("nearby-food");

app.Run();

public partial class Program
{
}
