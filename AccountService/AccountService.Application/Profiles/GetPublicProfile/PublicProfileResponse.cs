namespace AccountService.Application.Profiles.GetPublicProfile;

public sealed record PublicProfileResponse(
    Guid UserId,
    string DisplayName,
    string Slug,
    string? Bio,
    string? AvatarUrl,
    string? CountryCode,
    ProfileStatsResponse Stats);

public sealed record ProfileStatsResponse(
    int TotalAscents,
    int DistinctPeaks,
    int HighestAltitudeMeters,
    Guid? HighestPeakId,
    string? HighestPeakName,
    DateOnly? LastAscentDate);
