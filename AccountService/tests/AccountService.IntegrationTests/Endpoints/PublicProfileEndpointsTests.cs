using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Profiles.GetPublicProfile;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class PublicProfileEndpointsTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task GetPublicProfile_PublicProfile_ReturnsToAnonymousWithStats()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateClient();

        PublicProfileResponse? profile = await client.GetFromJsonAsync<PublicProfileResponse>(
            $"/api/profiles/{userId}");

        profile!.UserId.Should().Be(userId);
        profile.Stats.TotalAscents.Should().Be(0);
    }

    [Fact]
    public async Task GetPublicProfile_PrivateProfile_ReturnsNotFoundToStranger()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        await MakePrivateAsync(userId);
        using HttpClient anonymous = _factory.CreateClient();

        using HttpResponseMessage response = await anonymous.GetAsync($"/api/profiles/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPublicProfile_PrivateProfile_ReturnsToOwner()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        await MakePrivateAsync(userId);
        using HttpClient ownerClient = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await ownerClient.GetAsync($"/api/profiles/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPublicProfile_UnknownUser_ReturnsNotFound()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync($"/api/profiles/{ApiTestHelpers.NewUserId()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task MakePrivateAsync(Guid userId)
    {
        using HttpClient ownerClient = _factory.CreateAuthenticatedClient(userId);
        using HttpResponseMessage response = await ownerClient.PutJsonAsync(
            "/api/profiles/me",
            new { displayName = "Rubén", bio = (string?)null, countryCode = (string?)null, visibility = "Private" });

        response.EnsureSuccessStatusCode();
    }
}
