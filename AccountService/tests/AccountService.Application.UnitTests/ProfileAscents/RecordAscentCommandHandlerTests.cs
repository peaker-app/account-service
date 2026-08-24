using AccountService.Application.ProfileAscents.RecordAscent;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.ProfileAscents;

public sealed class RecordAscentCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IProfileAscentRepository _ascentRepository = Substitute.For<IProfileAscentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly RecordAscentCommandHandler _handler;
    private readonly Profile _profile = ProfileFactory.For(UserId);

    public RecordAscentCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(ProfileFactory.Now);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(_profile);
        _ascentRepository.GetByProfileAsync(_profile.Id, Arg.Any<CancellationToken>())
            .Returns([]);
        _handler = new RecordAscentCommandHandler(
            _profileRepository,
            _ascentRepository,
            _unitOfWork,
            _dateTimeProvider,
            NullLogger<RecordAscentCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithFirstAscent_AddsTheRecordAndRefreshesTheStats()
    {
        Result result = await _handler.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ascentRepository.Received(1).Add(Arg.Any<ProfileAscent>());
        _profile.Stats.Overall.Should().BeEquivalentTo(new { TotalAscents = 1, DistinctPeaks = 1 });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPrivateAscent_KeepsThePublicBlockEmpty()
    {
        await _handler.Handle(Command(visibility: nameof(AscentVisibility.Private)), CancellationToken.None);

        _profile.Stats.Public.Should().Be(ProfileStatsSnapshot.Empty);
    }

    [Fact]
    public async Task Handle_WithAlreadyProjectedAscent_SyncsItInsteadOfAddingAnother()
    {
        RecordAscentCommand command = Command();
        GivenTheAscentIsAlreadyProjected(command.AscentId, AscentVisibility.Private);

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ascentRepository.DidNotReceive().Add(Arg.Any<ProfileAscent>());
    }

    [Fact]
    public async Task Handle_WithAlreadyProjectedAscent_AdoptsTheReplayedVisibility()
    {
        RecordAscentCommand command = Command();
        GivenTheAscentIsAlreadyProjected(command.AscentId, AscentVisibility.Private);

        await _handler.Handle(command, CancellationToken.None);

        _profile.Stats.Public.TotalAscents.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithoutProfile_DiscardsTheEvent()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result result = await _handler.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutProfile_DoesNotPersistAnything()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        await _handler.Handle(Command(), CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void GivenTheAscentIsAlreadyProjected(Guid ascentId, AscentVisibility visibility)
    {
        ProfileAscent projected = ProfileAscent.Create(new ProfileAscentDraft(
            _profile.Id,
            ascentId,
            PeakSnapshot.Create(ProfileAscentFactory.AnetoId, "Aneto", 3404).Value,
            new DateOnly(2025, 7, 14),
            visibility)).Value;

        _ascentRepository.GetByProfileAsync(_profile.Id, Arg.Any<CancellationToken>())
            .Returns([projected]);
    }

    [Fact]
    public async Task Handle_WithUnknownVisibility_ReturnsVisibilityInvalid()
    {
        Result result = await _handler.Handle(Command(visibility: "FollowersOnly"), CancellationToken.None);

        result.Error.Should().Be(ProfileAscentErrors.VisibilityInvalid);
    }

    private static RecordAscentCommand Command(string visibility = nameof(AscentVisibility.Public)) => new(
        UserId,
        Guid.CreateVersion7(),
        ProfileAscentFactory.AnetoId,
        "Aneto",
        3404,
        new DateOnly(2025, 7, 14),
        visibility);
}
