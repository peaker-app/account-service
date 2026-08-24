namespace AccountService.Application.Profiles.GetMyProfile;

public sealed record ProfileResponse(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string Slug,
    string? Bio,
    string? AvatarUrl,
    string? CountryCode,
    string Visibility);
