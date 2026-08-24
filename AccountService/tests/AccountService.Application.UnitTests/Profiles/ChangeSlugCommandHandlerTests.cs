using AccountService.Application.Profiles.ChangeSlug;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class ChangeSlugCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeSlugCommandHandler _handler;

    public ChangeSlugCommandHandlerTests() =>
        _handler = new ChangeSlugCommandHandler(_profileRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithFreeSlug_ChangesItAndPersists()
    {
        Profile profile = ProfileFactory.For(UserId, "hiker");
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);
        _profileRepository.ExistsBySlugAsync("ruben-fer", Arg.Any<CancellationToken>()).Returns(false);

        Result result = await _handler.Handle(new ChangeSlugCommand(UserId, "ruben-fer"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.Slug.Value.Should().Be("ruben-fer");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTakenSlug_ReturnsConflict()
    {
        Profile profile = ProfileFactory.For(UserId, "hiker");
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);
        _profileRepository.ExistsBySlugAsync("ruben-fer", Arg.Any<CancellationToken>()).Returns(true);

        Result result = await _handler.Handle(new ChangeSlugCommand(UserId, "ruben-fer"), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.SlugAlreadyTaken);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidSlugFormat_ReturnsValidation()
    {
        Result result = await _handler.Handle(new ChangeSlugCommand(UserId, "Not Valid"), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.SlugInvalid);
        await _profileRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameSlug_IsNoOpSuccess()
    {
        Profile profile = ProfileFactory.For(UserId, "hiker");
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);

        Result result = await _handler.Handle(new ChangeSlugCommand(UserId, "hiker"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _profileRepository.DidNotReceive().ExistsBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
