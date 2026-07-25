using AccountService.Application.ProfileAscents.SyncAscent;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.ProfileAscents;

public sealed class SyncAscentCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly DateOnly NewDate = new(2026, 6, 1);

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IProfileAscentRepository _ascentRepository = Substitute.For<IProfileAscentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly SyncAscentCommandHandler _handler;
    private readonly Profile _profile = ProfileFactory.For(UserId);
    private readonly ProfileAscent _record;

    public SyncAscentCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(ProfileFactory.Now);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(_profile);
        _record = ProfileAscentFactory.For(_profile.Id, ascentDate: new DateOnly(2024, 1, 5));
        _ascentRepository.GetByAscentIdAsync(_record.AscentId, Arg.Any<CancellationToken>()).Returns(_record);
        _ascentRepository.GetByProfileAsync(_profile.Id, Arg.Any<CancellationToken>()).Returns([_record]);
        _handler = new SyncAscentCommandHandler(
            _profileRepository, _ascentRepository, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithANewDate_MovesTheLastAscentDate()
    {
        Result result = await _handler.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _profile.Stats.Overall.LastAscentDate.Should().Be(NewDate);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheAscentTurnsPrivate_DropsItFromThePublicBlock()
    {
        await _handler.Handle(Command(nameof(AscentVisibility.Private)), CancellationToken.None);

        _profile.Stats.Public.Should().Be(ProfileStatsSnapshot.Empty);
        _profile.Stats.Overall.TotalAscents.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenTheRecordIsMissing_IsIdempotentAndDoesNotSave()
    {
        _ascentRepository.GetByAscentIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ProfileAscent?)null);

        Result result = await _handler.Handle(
            new SyncAscentCommand(UserId, Guid.CreateVersion7(), NewDate, nameof(AscentVisibility.Public)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownVisibility_ReturnsVisibilityInvalid()
    {
        Result result = await _handler.Handle(Command("FollowersOnly"), CancellationToken.None);

        result.Error.Should().Be(ProfileAscentErrors.VisibilityInvalid);
    }

    private SyncAscentCommand Command(string visibility = nameof(AscentVisibility.Public)) =>
        new(UserId, _record.AscentId, NewDate, visibility);
}
