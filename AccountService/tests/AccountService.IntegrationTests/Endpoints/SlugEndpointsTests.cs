using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Profiles.GetPublicProfile;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class SlugEndpointsTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task ChangeSlug_WithFreeSlug_UpdatesPublicUrl()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);
        string newSlug = $"ruben-{Guid.CreateVersion7():N}"[..20];

        using HttpResponseMessage response = await client.PutJsonAsync("/api/profiles/me/slug", new { slug = newSlug });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using HttpResponseMessage lookup = await client.GetAsync($"/api/profiles/by-slug/{newSlug}");
        lookup.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeSlug_WithTakenSlug_ReturnsConflict()
    {
        Guid ownerOfSlug = ApiTestHelpers.NewUserId();
        Guid otherUser = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(ownerOfSlug, ApiTestHelpers.UniqueUsername());
        await _factory.SeedProfileAsync(otherUser, ApiTestHelpers.UniqueUsername());

        using HttpClient ownerClient = _factory.CreateAuthenticatedClient(ownerOfSlug);
        string takenSlug = $"taken-{Guid.CreateVersion7():N}"[..20];
        (await ownerClient.PutJsonAsync("/api/profiles/me/slug", new { slug = takenSlug })).Dispose();

        using HttpClient otherClient = _factory.CreateAuthenticatedClient(otherUser);
        using HttpResponseMessage response = await otherClient.PutJsonAsync(
            "/api/profiles/me/slug", new { slug = takenSlug });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ChangeSlug_WithInvalidFormat_ReturnsBadRequest()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.PutJsonAsync(
            "/api/profiles/me/slug", new { slug = "Not Valid" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPublicProfileBySlug_Unknown_ReturnsNotFound()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/profiles/by-slug/unknown-{Guid.CreateVersion7():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPublicProfileBySlug_Public_ReturnsProfile()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        string username = ApiTestHelpers.UniqueUsername();
        await _factory.SeedProfileAsync(userId, username);
        using HttpClient client = _factory.CreateClient();

        PublicProfileResponse? profile = await client.GetFromJsonAsync<PublicProfileResponse>(
            $"/api/profiles/by-slug/{username}");

        profile!.UserId.Should().Be(userId);
    }
}
