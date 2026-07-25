using AccountService.Application.Profiles.GetMyStats;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class GetMyStatsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly GetMyStatsQueryHandler _handler;

    public GetMyStatsQueryHandlerTests() => _handler = new GetMyStatsQueryHandler(_profileRepository);

    [Fact]
    public async Task Handle_WithExistingProfile_ReturnsTheOverallBlockIncludingPrivateAscents()
    {
        Profile profile = ProfileFactory.For(UserId);
        profile.RefreshStats(
            ProfileStatsCalculator.Calculate(
            [
                ProfileAscentFactory.For(profile.Id, peak: ProfileAscentFactory.Aneto),
                ProfileAscentFactory.For(
                    profile.Id, peak: ProfileAscentFactory.MontBlanc, visibility: AscentVisibility.Private)
            ]),
            ProfileFactory.Now);
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(profile);

        Result<ProfileStatsResponse> result = await _handler.Handle(
            new GetMyStatsQuery(UserId), CancellationToken.None);

        result.Value.Should().BeEquivalentTo(new
        {
            TotalAscents = 2,
            DistinctPeaks = 2,
            HighestAltitudeMeters = 4808,
            HighestPeakName = "Mont Blanc"
        });
    }

    [Fact]
    public async Task Handle_WithoutProfile_ReturnsNotFound()
    {
        _profileRepository.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Profile?)null);

        Result<ProfileStatsResponse> result = await _handler.Handle(
            new GetMyStatsQuery(UserId), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.NotFound(UserId));
    }
}
