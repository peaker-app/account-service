using AccountService.Application.Profiles.RemoveAvatar;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class RemoveAvatarCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemoveAvatarCommandHandler _handler;

    public RemoveAvatarCommandHandlerTests() =>
        _handler = new RemoveAvatarCommandHandler(_profileRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithAvatar_RemovesItAndPersists()
    {
        Profile profile = ProfileFactory.For(UserId);
        profile.SetAvatar(new Avatar("old-public-id", "https://cdn/old.webp"));
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);

        Result result = await _handler.Handle(new RemoveAvatarCommand(UserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.Avatar.Should().BeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileMissing_ReturnsNotFound()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result result = await _handler.Handle(new RemoveAvatarCommand(UserId), CancellationToken.None);

        result.Error.Type.Should().Be(Common.Domain.Results.ErrorType.NotFound);
    }
}
