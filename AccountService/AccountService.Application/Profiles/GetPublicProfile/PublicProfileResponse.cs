using AccountService.Application.Profiles.GetMyStats;

namespace AccountService.Application.Profiles.GetPublicProfile;

public sealed record PublicProfileResponse(
    Guid UserId,
    string DisplayName,
    string Slug,
    string? Bio,
    string? AvatarUrl,
    string? CountryCode,
    ProfileStatsResponse Stats);
