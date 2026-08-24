using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class ExportMyProfileEndpointTests(AccountServiceApiFactory factory)
{
    [Fact]
    public async Task Export_WithoutAToken_Returns401()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/profiles/me/export", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Export_WithoutAProfile_Returns404()
    {
        using HttpClient client = factory.CreateAuthenticatedClient(Guid.CreateVersion7());

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/profiles/me/export", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Export_ReturnsTheProfileItsStatsAndItsCollections()
    {
        Guid userId = Guid.CreateVersion7();
        await factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = factory.CreateAuthenticatedClient(userId);

        JsonElement export = await client.GetFromJsonAsync<JsonElement>("/api/profiles/me/export");

        export.GetProperty("profile").GetProperty("userId").GetGuid().Should().Be(userId);
        export.TryGetProperty("stats", out _).Should().BeTrue();
        export.GetProperty("collections").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Export_NeverIncludesAnotherClimbersCollections()
    {
        Guid mine = Guid.CreateVersion7();
        Guid theirs = Guid.CreateVersion7();
        await factory.SeedProfileAsync(mine, ApiTestHelpers.UniqueUsername());
        await factory.SeedProfileAsync(theirs, ApiTestHelpers.UniqueUsername());
        using HttpClient client = factory.CreateAuthenticatedClient(mine);

        JsonElement export = await client.GetFromJsonAsync<JsonElement>("/api/profiles/me/export");

        export.GetProperty("collections").GetArrayLength().Should().Be(1);
    }
}
