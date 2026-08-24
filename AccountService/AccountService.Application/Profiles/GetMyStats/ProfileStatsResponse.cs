namespace AccountService.Application.Profiles.GetMyStats;

public sealed record ProfileStatsResponse(
    int TotalAscents,
    int DistinctPeaks,
    int HighestAltitudeMeters,
    Guid? HighestPeakId,
    string? HighestPeakName,
    DateOnly? LastAscentDate);
