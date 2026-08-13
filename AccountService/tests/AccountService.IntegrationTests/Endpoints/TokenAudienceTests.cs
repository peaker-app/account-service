using System.Net;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class TokenAudienceTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task ProtectedRoute_WithATokenIssuedForAnotherService_Returns401()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateClientWithAudience(userId, "peaker-ascent");

        using HttpResponseMessage response = await client.GetAsync("/api/profiles/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedRoute_WithATokenIssuedForThisService_IsAccepted()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        using HttpClient client = _factory.CreateClientWithAudience(userId, "peaker-account");

        using HttpResponseMessage response = await client.GetAsync("/api/profiles/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
