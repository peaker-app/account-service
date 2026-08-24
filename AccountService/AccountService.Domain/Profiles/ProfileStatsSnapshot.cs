namespace AccountService.Domain.Profiles;

public sealed record ProfileStatsSnapshot(
    int TotalAscents,
    int DistinctPeaks,
    int HighestAltitudeMeters,
    Guid? HighestPeakId,
    string? HighestPeakName,
    DateOnly? LastAscentDate)
{
    public static readonly ProfileStatsSnapshot Empty = new(0, 0, 0, null, null, null);
}
