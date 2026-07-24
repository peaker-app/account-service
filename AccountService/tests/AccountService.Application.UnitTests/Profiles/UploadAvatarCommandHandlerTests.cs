using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.UploadAvatar;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class UploadAvatarCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly StoredImage Stored = new("peaker/dev/avatars/a1", "https://cdn/a1.webp");

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IImageStorage _imageStorage = Substitute.For<IImageStorage>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UploadAvatarCommandHandler _handler;

    public UploadAvatarCommandHandlerTests() =>
        _handler = new UploadAvatarCommandHandler(_profileRepository, _imageStorage, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidImage_StoresItAndSetsAvatar()
    {
        Profile profile = ProfileFactory.For(UserId);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);
        _imageStorage.UploadAvatarAsync(Arg.Any<AvatarUpload>(), Arg.Any<CancellationToken>()).Returns(Stored);

        Result<AvatarResponse> result = await _handler.Handle(
            new UploadAvatarCommand(UserId, AvatarUploads.ValidPng()), CancellationToken.None);

        result.Value.AvatarUrl.Should().Be(Stored.SecureUrl);
        profile.Avatar!.PublicId.Should().Be(Stored.PublicId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOversizedImage_ReturnsTooLargeWithoutUploading()
    {
        Result<AvatarResponse> result = await _handler.Handle(
            new UploadAvatarCommand(UserId, AvatarUploads.TooLarge()), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.AvatarTooLarge);
        await _imageStorage.DidNotReceive().UploadAvatarAsync(Arg.Any<AvatarUpload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnsupportedFormat_ReturnsFormatNotSupported()
    {
        Result<AvatarResponse> result = await _handler.Handle(
            new UploadAvatarCommand(UserId, AvatarUploads.UnsupportedFormat()), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.AvatarFormatNotSupported);
        await _imageStorage.DidNotReceive().UploadAvatarAsync(Arg.Any<AvatarUpload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileMissing_ReturnsNotFoundWithoutUploading()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result<AvatarResponse> result = await _handler.Handle(
            new UploadAvatarCommand(UserId, AvatarUploads.ValidPng()), CancellationToken.None);

        result.Error.Type.Should().Be(Common.Domain.Results.ErrorType.NotFound);
        await _imageStorage.DidNotReceive().UploadAvatarAsync(Arg.Any<AvatarUpload>(), Arg.Any<CancellationToken>());
    }
}
