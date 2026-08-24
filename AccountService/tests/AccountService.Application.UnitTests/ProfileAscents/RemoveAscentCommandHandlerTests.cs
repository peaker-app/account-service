using AccountService.Application.ProfileAscents.RemoveAscent;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.ProfileAscents;

public sealed class RemoveAscentCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IProfileAscentRepository _ascentRepository = Substitute.For<IProfileAscentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly RemoveAscentCommandHandler _handler;
    private readonly Profile _profile = ProfileFactory.For(UserId);

    public RemoveAscentCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(ProfileFactory.Now);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(_profile);
        _handler = new RemoveAscentCommandHandler(
            _profileRepository, _ascentRepository, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WhenTheRecordExists_RemovesItAndRecalculatesWithoutIt()
    {
        ProfileAscent removed = ProfileAscentFactory.For(_profile.Id, peak: ProfileAscentFactory.MontBlanc);
        ProfileAscent kept = ProfileAscentFactory.For(_profile.Id, peak: ProfileAscentFactory.Aneto);
        Given([removed, kept], removed);

        Result result = await _handler.Handle(new RemoveAscentCommand(UserId, removed.AscentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ascentRepository.Received(1).Remove(removed);
        _profile.Stats.Overall.Should().BeEquivalentTo(new
        {
            TotalAscents = 1,
            HighestPeakId = ProfileAscentFactory.AnetoId
        });
    }

    [Fact]
    public async Task Handle_WhenTheLastRecordIsRemoved_LeavesTheStatsEmpty()
    {
        ProfileAscent removed = ProfileAscentFactory.For(_profile.Id);
        Given([removed], removed);

        await _handler.Handle(new RemoveAscentCommand(UserId, removed.AscentId), CancellationToken.None);

        _profile.Stats.Overall.Should().Be(ProfileStatsSnapshot.Empty);
    }

    [Fact]
    public async Task Handle_WhenTheRecordIsMissing_IsIdempotentAndDoesNotSave()
    {
        _ascentRepository.GetByAscentIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ProfileAscent?)null);

        Result result = await _handler.Handle(
            new RemoveAscentCommand(UserId, Guid.CreateVersion7()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutProfile_ReturnsNotFound()
    {
        ProfileAscent removed = ProfileAscentFactory.For(_profile.Id);
        Given([removed], removed);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result result = await _handler.Handle(new RemoveAscentCommand(UserId, removed.AscentId), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.NotFound(UserId));
    }

    private void Given(ProfileAscent[] ascents, ProfileAscent target)
    {
        _ascentRepository.GetByAscentIdAsync(target.AscentId, Arg.Any<CancellationToken>()).Returns(target);
        _ascentRepository.GetByProfileAsync(_profile.Id, Arg.Any<CancellationToken>()).Returns(ascents);
    }
}
