using System.Net;
using System.Net.Http.Json;
using EatThis.Api.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class ProxyRestrictionTests
{
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
                apiKey = "browser-supplied-secret",
                providerUrl = "https://attacker.example/forward",
                fieldMask = "*",
                provider = "osm",
            });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, factory.ProviderCalls);
        Assert.AreEqual(3000, factory.LastQuery?.RadiusMeters);

        var body = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(body.Contains("browser-supplied-secret", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("attacker.example", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("raw provider", StringComparison.Ordinal));
        var selected = await response.Content.ReadFromJsonAsync<PlaceCandidate>();
        Assert.IsNotNull(selected);
        Assert.AreEqual("google", selected!.Provider);
    }
}
