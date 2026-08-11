using AccountService.Infrastructure.ExternalServices;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.ExternalServices;

public sealed class AvatarDeliveryTests
{
    [Fact]
    public void Sanitizing_StripsTheColourProfile() =>
        AvatarDelivery.Sanitizing().ToString().Should().Contain("strip_profile");

    [Fact]
    public void Sanitizing_StripsTheExifBlock() =>
        AvatarDelivery.Sanitizing().ToString().Should().Contain("strip_exif");

    [Fact]
    public void StoredFormat_ForcesARecodeSoTheStoredBytesAreNeverTheUploadedOnes() =>
        AvatarDelivery.StoredFormat.Should().Be("webp");
}
