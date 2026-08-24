using AccountService.Application.Profiles.GetPublicProfile;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class GetPublicProfileByIdQueryHandlerTests
{
    private static readonly Guid TargetUserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly GetPublicProfileByIdQueryHandler _handler;

    public GetPublicProfileByIdQueryHandlerTests() =>
        _handler = new GetPublicProfileByIdQueryHandler(_profileRepository, ProfileFactory.AvatarUrlSigner());

    [Fact]
    public async Task Handle_PublicProfile_ReturnsResponseToAnonymous()
    {
        _profileRepository.GetByUserIdAsync(TargetUserId, Arg.Any<CancellationToken>())
            .Returns(ProfileFactory.For(TargetUserId, "hiker"));

        Result<PublicProfileResponse> result = await _handler.Handle(
            new GetPublicProfileByIdQuery(TargetUserId, RequesterId: null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be("hiker");
    }

    [Fact]
    public async Task Handle_PrivateProfile_ReturnsNotFoundToOtherUser()
    {
        _profileRepository.GetByUserIdAsync(TargetUserId, Arg.Any<CancellationToken>())
            .Returns(PrivateProfile());

        Result<PublicProfileResponse> result = await _handler.Handle(
            new GetPublicProfileByIdQuery(TargetUserId, Guid.CreateVersion7()), CancellationToken.None);

        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_PrivateProfile_ReturnsResponseToOwner()
    {
        _profileRepository.GetByUserIdAsync(TargetUserId, Arg.Any<CancellationToken>())
            .Returns(PrivateProfile());

        Result<PublicProfileResponse> result = await _handler.Handle(
            new GetPublicProfileByIdQuery(TargetUserId, TargetUserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MissingProfile_ReturnsNotFound()
    {
        _profileRepository.GetByUserIdAsync(TargetUserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result<PublicProfileResponse> result = await _handler.Handle(
            new GetPublicProfileByIdQuery(TargetUserId, RequesterId: null), CancellationToken.None);

        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    private static Profile PrivateProfile()
    {
        Profile profile = ProfileFactory.For(TargetUserId, "hiker");
        profile.UpdateDetails(new ProfileDetails(
            DisplayName.Create("hiker").Value, null, null, ProfileVisibility.Private));

        return profile;
    }
}
