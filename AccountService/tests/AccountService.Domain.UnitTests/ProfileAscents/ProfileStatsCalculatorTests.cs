using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using AccountService.Domain.UnitTests.TestData;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.ProfileAscents;

public sealed class ProfileStatsCalculatorTests
{
    [Fact]
    public void Calculate_WithoutAscents_ReturnsEmptyStats()
    {
        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate([]);

        update.Should().Be(ProfileStatsUpdate.Empty);
    }

    [Fact]
    public void Calculate_WithRepeatedPeak_CountsTotalAndDistinctSeparately()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ProfileAscentMother.Aneto),
            ProfileAscentMother.Create(ProfileAscentMother.Aneto),
            ProfileAscentMother.Create(ProfileAscentMother.MontBlanc)
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Overall.Should().BeEquivalentTo(new { TotalAscents = 3, DistinctPeaks = 2 });
    }

    [Fact]
    public void Calculate_WithSeveralPeaks_ReturnsTheHighestOneWithItsIdentity()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ProfileAscentMother.Aneto),
            ProfileAscentMother.Create(ProfileAscentMother.MontBlanc)
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Overall.Should().BeEquivalentTo(new
        {
            HighestAltitudeMeters = 4808,
            HighestPeakId = ProfileAscentMother.MontBlancId,
            HighestPeakName = "Mont Blanc"
        });
    }

    [Fact]
    public void Calculate_WithSeveralDates_ReturnsTheMostRecentAsLastAscent()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ascentDate: new DateOnly(2024, 5, 1)),
            ProfileAscentMother.Create(ascentDate: new DateOnly(2026, 2, 9)),
            ProfileAscentMother.Create(ascentDate: new DateOnly(2025, 8, 20))
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Overall.LastAscentDate.Should().Be(new DateOnly(2026, 2, 9));
    }

    [Fact]
    public void Calculate_WithTwoAscentsOfTheSameAltitude_KeepsTheMostRecentAsRecord()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ProfileAscentMother.Aneto, new DateOnly(2024, 5, 1)),
            ProfileAscentMother.Create(ProfileAscentMother.Aneto, new DateOnly(2026, 2, 9))
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Overall.HighestPeakId.Should().Be(ProfileAscentMother.AnetoId);
    }

    [Fact]
    public void Calculate_WithPrivateAscents_ExcludesThemFromThePublicBlock()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ProfileAscentMother.Aneto),
            ProfileAscentMother.Create(ProfileAscentMother.MontBlanc, visibility: AscentVisibility.Private)
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Public.Should().BeEquivalentTo(new
        {
            TotalAscents = 1,
            DistinctPeaks = 1,
            HighestAltitudeMeters = 3404,
            HighestPeakId = ProfileAscentMother.AnetoId,
            HighestPeakName = "Aneto"
        });
    }

    [Fact]
    public void Calculate_WithPrivateAscentAsTheMostRecent_HidesItsDateFromThePublicBlock()
    {
        ProfileAscent[] ascents =
        [
            ProfileAscentMother.Create(ascentDate: new DateOnly(2025, 8, 20)),
            ProfileAscentMother.Create(
                ascentDate: new DateOnly(2026, 2, 9), visibility: AscentVisibility.Private)
        ];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Public.LastAscentDate.Should().Be(new DateOnly(2025, 8, 20));
    }

    [Fact]
    public void Calculate_WithEveryAscentPrivate_ReturnsAnEmptyPublicBlock()
    {
        ProfileAscent[] ascents = [ProfileAscentMother.Create(visibility: AscentVisibility.Private)];

        ProfileStatsUpdate update = ProfileStatsCalculator.Calculate(ascents);

        update.Public.Should().Be(ProfileStatsSnapshot.Empty);
    }
}
