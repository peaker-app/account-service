using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Profiles.GetMyProfile;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class AvatarEndpointsTests(AccountServiceApiFactory factory)
{
    private static readonly byte[] PngContent = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01];
    private static readonly byte[] UnsupportedContent = [0x00, 0x01, 0x02, 0x03];

    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task UploadAvatar_WithValidPng_StoresUrlOnProfile()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.UploadAvatarAsync(PngContent, "image/png", "avatar.png");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ProfileResponse? profile = await client.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");
        profile!.AvatarUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UploadAvatar_WithUnsupportedFormat_ReturnsBadRequest()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.UploadAvatarAsync(
            UnsupportedContent, "application/octet-stream", "avatar.bin");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveAvatar_AfterUpload_ClearsProfileAvatar()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);
        (await client.UploadAvatarAsync(PngContent, "image/png", "avatar.png")).Dispose();

        using HttpResponseMessage response = await client.DeleteAsync("/api/profiles/me/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        ProfileResponse? profile = await client.GetFromJsonAsync<ProfileResponse>("/api/profiles/me");
        profile!.AvatarUrl.Should().BeNull();
    }
}
