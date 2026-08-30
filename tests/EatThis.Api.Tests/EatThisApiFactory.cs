using EatThis.Api.Application;
using EatThis.Api.Contracts;
using EatThis.Api.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EatThis.Api.Tests;

internal sealed class EatThisApiFactory : WebApplicationFactory<Program>
{
    private readonly ExamplePlaceProvider provider = new();

    public int ProviderCalls => provider.CallCount;

    public NearbySearchQuery? LastQuery => provider.LastQuery;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPlaceProvider>();
            services.AddSingleton<IPlaceProvider>(provider);
            services.RemoveAll<IRandomSource>();
            services.AddSingleton<IRandomSource, FirstCandidateRandomSource>();
        });
    }

    private sealed class ExamplePlaceProvider : IPlaceProvider
    {
        public int CallCount { get; private set; }

        public NearbySearchQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<PlaceCandidate>> SearchAsync(
            NearbySearchQuery query,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastQuery = query;
            IReadOnlyList<PlaceCandidate> places =
            [
                new(
                    "Example Food Shop",
                    "Taipei City",
                    25.0331,
                    121.5655,
                    420,
                    "https://www.google.com/maps/place/example",
                    "google"),
            ];
            return Task.FromResult(places);
        }
    }

    private sealed class FirstCandidateRandomSource : IRandomSource
    {
        public int Next(int maxExclusive) => 0;
    }
}
