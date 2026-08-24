using AccountService.Application.Profiles.UpdateProfile;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class UpdateProfileCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests() =>
        _handler = new UpdateProfileCommandHandler(_profileRepository, _unitOfWork);

    private static UpdateProfileCommand Command(
        string displayName = "Rubén",
        string? bio = "Cimas y valles.",
        string? countryCode = "ES",
        ProfileVisibility visibility = ProfileVisibility.Public) =>
        new(UserId, displayName, bio, countryCode, visibility);

    [Fact]
    public async Task Handle_WithValidData_UpdatesProfileAndPersists()
    {
        Profile profile = ProfileFactory.For(UserId);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);

        Result result = await _handler.Handle(Command(visibility: ProfileVisibility.Private), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.Visibility.Should().Be(ProfileVisibility.Private);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileMissing_ReturnsNotFound()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result result = await _handler.Handle(Command(), CancellationToken.None);

        result.Error.Type.Should().Be(Common.Domain.Results.ErrorType.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCountryCode_ReturnsValidationWithoutLoadingProfile()
    {
        Result result = await _handler.Handle(Command(countryCode: "ZZ"), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.CountryCodeInvalid);
        await _profileRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithBioOverLimit_ReturnsBioTooLong()
    {
        Profile profile = ProfileFactory.For(UserId);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);

        Result result = await _handler.Handle(
            Command(bio: new string('a', Profile.MaxBioLength + 1)), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.BioTooLong);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
