using AccountService.Domain.Profiles;

namespace AccountService.Domain.ProfileAscents;

public static class ProfileStatsCalculator
{
    public static ProfileStatsUpdate Calculate(IReadOnlyCollection<ProfileAscent> ascents) => new(
        Summarize(ascents),
        Summarize([.. ascents.Where(ascent => ascent.IsPublic)]));

    private static ProfileStatsSnapshot Summarize(IReadOnlyCollection<ProfileAscent> ascents)
    {
        if (ascents.Count == 0)
        {
            return ProfileStatsSnapshot.Empty;
        }

        ProfileAscent highest = ascents
            .OrderByDescending(ascent => ascent.Peak.AltitudeMeters)
            .ThenByDescending(ascent => ascent.AscentDate)
            .First();

        return new ProfileStatsSnapshot(
            ascents.Count,
            ascents.Select(ascent => ascent.Peak.PeakId).Distinct().Count(),
            highest.Peak.AltitudeMeters,
            highest.Peak.PeakId,
            highest.Peak.Name,
            ascents.Max(ascent => ascent.AscentDate));
    }
}
