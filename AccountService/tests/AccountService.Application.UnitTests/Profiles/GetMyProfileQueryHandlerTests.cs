using AccountService.Application.Profiles.GetMyProfile;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class GetMyProfileQueryHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests() =>
        _handler = new GetMyProfileQueryHandler(_profileRepository);

    [Fact]
    public async Task Handle_WithExistingProfile_ReturnsOwnData()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(ProfileFactory.For(UserId, "hiker"));

        Result<ProfileResponse> result = await _handler.Handle(new GetMyProfileQuery(UserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(UserId);
        result.Value.Slug.Should().Be("hiker");
    }

    [Fact]
    public async Task Handle_WhenProfileMissing_ReturnsNotFound()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result<ProfileResponse> result = await _handler.Handle(new GetMyProfileQuery(UserId), CancellationToken.None);

        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
