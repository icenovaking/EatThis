using System.Net;
using System.Net.Http.Json;
using EatThis.Api.Contracts;
using EatThis.Api.Application;
using EatThis.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class ProxyRestrictionTests
{
    [TestMethod]
    public async Task Unsupported_filters_cannot_override_actual_google_category_request()
    {
        var handler = new RecordingGoogleHandler();
        await using var factory = new GoogleApiFactory(handler);
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/nearby-food/pick", new
        {
            latitude = 25.033, longitude = 121.5654, radiusMeters = 1000, minRating = 4,
            restaurantCategory = "japanese", includedTypes = new[] { "bar" },
            includedPrimaryTypes = new[] { "bar" }, excludedTypes = new[] { "japanese_restaurant" },
            fieldMask = "*", languageCode = "en-US", maxResultCount = 100,
        });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.AreEqual("no_results", error?.ErrorCode);
        Assert.AreEqual(1, handler.CallCount);
        using var json = JsonDocument.Parse(handler.Body!);
        var root = json.RootElement;
        CollectionAssert.AreEqual(new[] { "japanese_restaurant", "sushi_restaurant", "ramen_restaurant" },
            root.GetProperty("includedTypes").EnumerateArray().Select(item => item.GetString()).ToArray());
        Assert.AreEqual("zh-TW", root.GetProperty("languageCode").GetString());
        Assert.AreEqual(20, root.GetProperty("maxResultCount").GetInt32());
        Assert.AreEqual("places.displayName,places.formattedAddress,places.location,places.googleMapsUri,places.rating", handler.FieldMask);
        CollectionAssert.AreEquivalent(new[] { "includedTypes", "maxResultCount", "languageCode", "locationRestriction" },
            root.EnumerateObject().Select(property => property.Name).ToArray());
    }

    private sealed class GoogleApiFactory(RecordingGoogleHandler handler) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlaceProvider>();
                services.AddSingleton<IPlaceProvider>(_ => new GooglePlacesProvider(
                    new HttpClient(handler) { BaseAddress = GooglePlacesProvider.DefaultBaseAddress },
                    Options.Create(new GooglePlacesOptions { ApiKey = "test-key" }),
                    NullLogger<GooglePlacesProvider>.Instance));
            });
        }
    }

    private sealed class RecordingGoogleHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public string? Body { get; private set; }
        public string? FieldMask { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            FieldMask = request.Headers.GetValues("X-Goog-FieldMask").Single();
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"places\":[]}") };
        }
    }

    [TestMethod]
    public async Task Unsupported_proxy_fields_are_not_forwarded_or_exposed()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync(
            "/api/nearby-food/pick",
            new
            {
                latitude = 25.0330,
                longitude = 121.5654,
                radiusMeters = 3000,
                restaurantCategory = "japanese",
                includedTypes = new[] { "bar" },
                includedPrimaryTypes = new[] { "bar" },
                excludedTypes = new[] { "japanese_restaurant" },
                apiKey = "browser-supplied-secret",
                providerUrl = "https://attacker.example/forward",
                fieldMask = "*",
                provider = "osm",
            });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(3000, factory.LastQuery?.RadiusMeters);
        Assert.AreEqual(EatThis.Api.Domain.RestaurantCategory.Japanese, factory.LastQuery?.RestaurantCategory);

        var body = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(body.Contains("browser-supplied-secret", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("attacker.example", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("raw provider", StringComparison.Ordinal));
        var selected = await response.Content.ReadFromJsonAsync<PlaceCandidate>();
        Assert.IsNotNull(selected);
        Assert.AreEqual("google", selected!.Provider);
    }
}
