using AccountService.Domain.Profiles;

namespace AccountService.Application.Profiles.Mappings;

internal static class AvatarDeliveryLifetime
{
    private static readonly TimeSpan PublicProfile = TimeSpan.FromHours(24);
    private static readonly TimeSpan PrivateProfile = TimeSpan.FromMinutes(10);

    public static TimeSpan For(ProfileVisibility visibility) =>
        visibility is ProfileVisibility.Public ? PublicProfile : PrivateProfile;
}
