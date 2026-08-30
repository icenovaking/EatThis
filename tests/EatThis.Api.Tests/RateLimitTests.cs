using System.Net;
using System.Net.Http.Json;
using EatThis.Api.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class RateLimitTests
{
    [TestMethod]
    public async Task Exceeding_the_backend_limit_returns_retryable_429()
    {
        await using var factory = new EatThisApiFactory();
        using var client = factory.CreateClient();
        HttpResponseMessage? lastResponse = null;

        for (var requestNumber = 1; requestNumber <= 31; requestNumber++)
        {
            lastResponse?.Dispose();
            lastResponse = await client.PostAsJsonAsync(
                "/api/nearby-food/pick",
                new { latitude = 25.0330, longitude = 121.5654, radiusMeters = 3000 });

            if (requestNumber < 31)
            {
                Assert.AreEqual(HttpStatusCode.OK, lastResponse.StatusCode, $"Request {requestNumber}");
            }
        }

        using (var response = lastResponse!)
        {
            Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.IsTrue(response.Headers.Contains("Retry-After"));
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            Assert.IsNotNull(error);
            Assert.AreEqual("rate_limited", error.ErrorCode);
            Assert.AreEqual(60, error.RetryAfterSeconds);
        }
    }
}
