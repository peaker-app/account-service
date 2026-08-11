using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using AccountService.Application.ProfileAscents.RebuildProfileStats;
using AccountService.Domain.Profiles;
using AccountService.IntegrationTests.TestData;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class AdminProfilesEndpointTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task RebuildStats_WithoutAToken_Returns401()
    {
        using HttpClient anonymous = _factory.CreateClient();

        HttpResponseMessage response = await anonymous.PostAsync(RebuildRoute(Guid.CreateVersion7()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RebuildStats_WithoutTheAdminRole_Returns403()
    {
        using HttpClient hiker = _factory.CreateAuthenticatedClient(Guid.CreateVersion7());

        HttpResponseMessage response = await hiker.PostAsync(RebuildRoute(Guid.CreateVersion7()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RebuildStats_ForAnUnknownProfile_Returns404()
    {
        using HttpClient admin = _factory.CreateAdminClient(Guid.CreateVersion7());

        HttpResponseMessage response = await admin.PostAsync(RebuildRoute(Guid.CreateVersion7()), null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RebuildStats_AsAdmin_ReportsTheProjectedAscents()
    {
        Guid userId = await SeededProfileWithOneAscentAsync();
        using HttpClient admin = _factory.CreateAdminClient(Guid.CreateVersion7());

        HttpResponseMessage response = await admin.PostAsync(RebuildRoute(userId), null);
        ProfileStatsRebuildResponse? rebuild =
            await response.Content.ReadFromJsonAsync<ProfileStatsRebuildResponse>();

        rebuild!.AscentsProjected.Should().Be(1);
    }

    [Fact]
    public async Task RebuildStats_AsAdmin_LeavesTheStatsUnchangedWhenTheyWereCorrect()
    {
        Guid userId = await SeededProfileWithOneAscentAsync();
        using HttpClient admin = _factory.CreateAdminClient(Guid.CreateVersion7());

        await admin.PostAsync(RebuildRoute(userId), null);

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, _ => true);
        stats!.Overall.TotalAscents.Should().Be(1);
    }

    private async Task<Guid> SeededProfileWithOneAscentAsync()
    {
        Guid userId = Guid.CreateVersion7();
        await _factory.SeedProfileAsync(userId, $"hiker{userId:N}"[..20]);

        await _factory.PublishAsync(
            AscentEvents.Registered(userId, Guid.CreateVersion7(), Guid.CreateVersion7(), 3404));

        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);

        return userId;
    }

    private static string RebuildRoute(Guid userId) =>
        string.Create(CultureInfo.InvariantCulture, $"/api/admin/profiles/{userId}/stats/rebuild");
}
