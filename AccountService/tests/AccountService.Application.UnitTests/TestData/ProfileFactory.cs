using AccountService.Domain.Profiles;

namespace AccountService.Application.UnitTests.TestData;

internal static class ProfileFactory
{
    public static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static Profile For(Guid userId, string username = "hiker")
    {
        DisplayName displayName = DisplayName.Create(username).Value;
        ProfileSlug slug = ProfileSlug.FromUsername(username);

        return Profile.Create(new ProfileDraft(userId, displayName, slug), Now).Value;
    }
}
