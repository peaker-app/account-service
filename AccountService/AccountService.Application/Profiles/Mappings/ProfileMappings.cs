using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.GetMyProfile;
using AccountService.Application.Profiles.GetMyStats;
using AccountService.Application.Profiles.GetPublicProfile;
using AccountService.Domain.Profiles;

namespace AccountService.Application.Profiles.Mappings;

internal static class ProfileMappings
{
    public static ProfileResponse ToResponse(this Profile profile, IAvatarUrlSigner signer) => new(
        profile.Id,
        profile.UserId,
        profile.DisplayName.Value,
        profile.Slug.Value,
        profile.Bio,
        profile.SignAvatarUrl(signer),
        profile.CountryCode?.Value,
        profile.Visibility.ToString());

    public static ProfileStatsResponse ToResponse(this ProfileStatsSnapshot stats) => new(
        stats.TotalAscents,
        stats.DistinctPeaks,
        stats.HighestAltitudeMeters,
        stats.HighestPeakId,
        stats.HighestPeakName,
        stats.LastAscentDate);

    public static PublicProfileResponse ToPublicResponse(this Profile profile, IAvatarUrlSigner signer) => new(
        profile.UserId,
        profile.DisplayName.Value,
        profile.Slug.Value,
        profile.Bio,
        profile.SignAvatarUrl(signer),
        profile.CountryCode?.Value,
        profile.Stats.Public.ToResponse());

    public static string? SignAvatarUrl(this Profile profile, IAvatarUrlSigner signer) =>
        profile.Avatar is null
            ? null
            : signer.Sign(profile.Avatar.PublicId, AvatarDeliveryLifetime.For(profile.Visibility));
}
