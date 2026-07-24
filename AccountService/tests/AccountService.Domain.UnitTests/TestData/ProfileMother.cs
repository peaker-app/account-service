using AccountService.Domain.Profiles;

namespace AccountService.Domain.UnitTests.TestData;

internal static class ProfileMother
{
    public static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public const string DefaultUsername = "hiker_ruben";

    public static Profile Create(string username = DefaultUsername)
    {
        DisplayName displayName = DisplayName.Create(username).Value;
        ProfileSlug slug = ProfileSlug.FromUsername(username);

        return Profile.Create(new ProfileDraft(Guid.CreateVersion7(), displayName, slug), Now).Value;
    }

    public static ProfileDetails Details(
        string displayName = "Rubén",
        string? bio = "Montañero de fin de semana.",
        string? countryCode = "ES",
        ProfileVisibility visibility = ProfileVisibility.Public) =>
        new(
            DisplayName.Create(displayName).Value,
            bio,
            countryCode is null ? null : CountryCode.Create(countryCode).Value,
            visibility);
}
