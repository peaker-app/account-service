using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Profiles.GetMyProfile;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class ProfileEndpointsTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task GetMyProfile_WithoutToken_ReturnsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/profiles/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_Authenticated_ReturnsOwnProfileFromToken()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        string username = ApiTestHelpers.UniqueUsername();
        await _factory.SeedProfileAsync(userId, username);
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        ProfileResponse? profile = await client.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");

        profile!.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be(username);
    }

    [Fact]
    public async Task GetMyProfile_TwoUsers_EachSeesOnlyTheirOwn()
    {
        Guid firstUser = ApiTestHelpers.NewUserId();
        Guid secondUser = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(firstUser, ApiTestHelpers.UniqueUsername());
        await _factory.SeedProfileAsync(secondUser, ApiTestHelpers.UniqueUsername());

        using HttpClient firstClient = _factory.CreateAuthenticatedClient(firstUser);
        using HttpClient secondClient = _factory.CreateAuthenticatedClient(secondUser);
        ProfileResponse? first = await firstClient.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");
        ProfileResponse? second = await secondClient.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");

        first!.UserId.Should().Be(firstUser);
        second!.UserId.Should().Be(secondUser);
    }

    [Fact]
    public async Task UpdateMyProfile_WithValidData_PersistsChanges()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.PutJsonAsync(
            "/api/profiles/me",
            new { displayName = "Rubén", bio = "Cimas y valles.", countryCode = "ES", visibility = "Private" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        ProfileResponse? profile = await client.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");
        profile!.DisplayName.Should().Be("Rubén");
        profile.CountryCode.Should().Be("ES");
        profile.Visibility.Should().Be("Private");
    }

    [Fact]
    public async Task UpdateMyProfile_WithInvalidCountry_ReturnsBadRequest()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.PutJsonAsync(
            "/api/profiles/me",
            new { displayName = "Rubén", bio = (string?)null, countryCode = "ZZ", visibility = "Public" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
