using AccountService.Application.ProfileAscents.SyncPeakName;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.ProfileAscents;

public sealed class SyncPeakNameCommandHandlerTests
{
    private static readonly SyncPeakNameCommand Command =
        new(ProfileAscentFactory.AnetoId, "Pico de Aneto", 3410);

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IProfileAscentRepository _ascentRepository = Substitute.For<IProfileAscentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly SyncPeakNameCommandHandler _handler;
    private readonly Profile _profile = ProfileFactory.For(Guid.CreateVersion7());

    public SyncPeakNameCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(ProfileFactory.Now);
        _profileRepository.GetByIdAsync(_profile.Id, Arg.Any<CancellationToken>()).Returns(_profile);
        _handler = new SyncPeakNameCommandHandler(
            _profileRepository, _ascentRepository, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithAffectedRecords_RenamesTheDenormalisedPeak()
    {
        ProfileAscent record = GivenAscentOfTheRenamedPeak();

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        record.Peak.Name.Should().Be("Pico de Aneto");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAffectedRecords_RefreshesTheStatsOfTheOwner()
    {
        GivenAscentOfTheRenamedPeak();

        await _handler.Handle(Command, CancellationToken.None);

        _profile.Stats.Overall.Should().BeEquivalentTo(new
        {
            HighestPeakName = "Pico de Aneto",
            HighestAltitudeMeters = 3410
        });
    }

    [Fact]
    public async Task Handle_WithoutAffectedRecords_DoesNothing()
    {
        _ascentRepository.GetByPeakAsync(Command.PeakId, Arg.Any<CancellationToken>()).Returns([]);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private ProfileAscent GivenAscentOfTheRenamedPeak()
    {
        ProfileAscent record = ProfileAscentFactory.For(_profile.Id, peak: ProfileAscentFactory.Aneto);
        _ascentRepository.GetByPeakAsync(Command.PeakId, Arg.Any<CancellationToken>()).Returns([record]);
        _ascentRepository.GetByProfileAsync(_profile.Id, Arg.Any<CancellationToken>()).Returns([record]);

        return record;
    }
}
