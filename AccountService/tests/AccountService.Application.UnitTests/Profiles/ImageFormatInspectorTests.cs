using AccountService.Application.Profiles.UploadAvatar;
using FluentAssertions;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class ImageFormatInspectorTests
{
    [Fact]
    public void IsSupported_JpegMagicBytes_ReturnsTrue() =>
        ImageFormatInspector.IsSupported([0xFF, 0xD8, 0xFF, 0x00]).Should().BeTrue();

    [Fact]
    public void IsSupported_PngMagicBytes_ReturnsTrue() =>
        ImageFormatInspector.IsSupported([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]).Should().BeTrue();

    [Fact]
    public void IsSupported_WebpMagicBytes_ReturnsTrue() =>
        ImageFormatInspector.IsSupported(
            [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50]).Should().BeTrue();

    [Fact]
    public void IsSupported_GifMagicBytes_ReturnsFalse() =>
        ImageFormatInspector.IsSupported([0x47, 0x49, 0x46, 0x38]).Should().BeFalse();

    [Fact]
    public void IsSupported_EmptyContent_ReturnsFalse() =>
        ImageFormatInspector.IsSupported([]).Should().BeFalse();
}
