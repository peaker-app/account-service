using AccountService.Infrastructure.ExternalServices;
using Common.Infrastructure.Time;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AccountService.IntegrationTests.ExternalServices;

public sealed class CloudinaryAvatarUrlSignerTests
{
    private const string PublicId = "peaker/test/avatars/avatar";
    private const string AuthTokenMarker = "__cld_token__";

    [Fact]
    public void Sign_WithAnAuthTokenKey_IssuesAnExpiringToken() =>
        SignerWith("00112233445566778899aabbccddeeff").Sign(PublicId, TimeSpan.FromMinutes(10))
            .Should().Contain(AuthTokenMarker);

    [Fact]
    public void Sign_WithoutAnAuthTokenKey_SignsTheUrlWithTheApiSecret() =>
        SignerWith(string.Empty).Sign(PublicId, TimeSpan.FromMinutes(10))
            .Should().Contain("/image/authenticated/s--");

    [Fact]
    public void Sign_WithoutAnAuthTokenKey_IssuesNoToken() =>
        SignerWith(string.Empty).Sign(PublicId, TimeSpan.FromMinutes(10))
            .Should().NotContain(AuthTokenMarker);

    [Fact]
    public void Sign_WithoutAnAuthTokenKey_NeverReturnsAPubliclyDeliverableUrl() =>
        SignerWith(string.Empty).Sign(PublicId, TimeSpan.FromMinutes(10))
            .Should().NotContain("/image/upload/");

    private static CloudinaryAvatarUrlSigner SignerWith(string authTokenKey)
    {
        CloudinaryOptions options = new()
        {
            CloudName = "peaker-test",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            AuthTokenKey = authTokenKey
        };

        return new CloudinaryAvatarUrlSigner(new CloudinaryFactory(Options.Create(options)), new DateTimeProvider());
    }
}
