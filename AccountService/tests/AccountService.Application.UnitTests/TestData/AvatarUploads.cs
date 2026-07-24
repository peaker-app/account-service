using AccountService.Application.Abstractions;

namespace AccountService.Application.UnitTests.TestData;

internal static class AvatarUploads
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static AvatarUpload ValidPng() =>
        new(PngHeader, "image/png", "avatar.png");

    public static AvatarUpload UnsupportedFormat() =>
        new(new byte[] { 0x00, 0x01, 0x02, 0x03 }, "application/octet-stream", "avatar.bin");

    public static AvatarUpload TooLarge()
    {
        var content = new byte[AccountService.Domain.Profiles.AvatarConstraints.MaxSizeBytes + 1];
        PngHeader.CopyTo(content, 0);

        return new AvatarUpload(content, "image/png", "avatar.png");
    }
}
