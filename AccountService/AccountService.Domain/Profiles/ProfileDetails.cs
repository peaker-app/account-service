namespace AccountService.Domain.Profiles;

public sealed record ProfileDetails(
    DisplayName DisplayName,
    string? Bio,
    CountryCode? CountryCode,
    ProfileVisibility Visibility);
