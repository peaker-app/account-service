using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Profiles.GetMyStats;
using AccountService.IntegrationTests.TestData;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class StatsEndpointTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task GetMyStats_WithoutAscents_ReturnsZeroedStats()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        ProfileStatsResponse? stats = await client.GetFromJsonAsync<ProfileStatsResponse>("/api/profiles/me/stats");

        stats!.Should().BeEquivalentTo(new
        {
            TotalAscents = 0,
            DistinctPeaks = 0,
            HighestAltitudeMeters = 0,
            HighestPeakId = (Guid?)null,
            LastAscentDate = (DateOnly?)null
        });
    }

    [Fact]
    public async Task GetMyStats_AfterRegisteringAscents_ReturnsTotalsIncludingPrivateOnes()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        Guid aneto = Guid.CreateVersion7();
        Guid montBlanc = Guid.CreateVersion7();

        await _factory.PublishAsync(
            AscentEvents.Registered(userId, Guid.CreateVersion7(), aneto, 3404));
        await _factory.PublishAsync(AscentEvents.Registered(
            userId, Guid.CreateVersion7(), montBlanc, 4808, AscentEvents.Private, "Mont Blanc"));

        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 2);
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);
        ProfileStatsResponse? stats = await client.GetFromJsonAsync<ProfileStatsResponse>("/api/profiles/me/stats");

        stats!.Should().BeEquivalentTo(new
        {
            TotalAscents = 2,
            DistinctPeaks = 2,
            HighestAltitudeMeters = 4808,
            HighestPeakId = montBlanc,
            HighestPeakName = "Mont Blanc"
        });
    }

    [Fact]
    public async Task GetMyStats_WithoutToken_ReturnsUnauthorized()
    {
        using HttpClient anonymous = _factory.CreateClient();

        using HttpResponseMessage response = await anonymous.GetAsync("/api/profiles/me/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyStats_WithoutProfile_ReturnsNotFound()
    {
        using HttpClient client = _factory.CreateAuthenticatedClient(ApiTestHelpers.NewUserId());

        using HttpResponseMessage response = await client.GetAsync("/api/profiles/me/stats");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
