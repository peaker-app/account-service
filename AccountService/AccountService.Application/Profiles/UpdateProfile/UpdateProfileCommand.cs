using AccountService.Domain.Profiles;
using Common.Application.Messaging;

namespace AccountService.Application.Profiles.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string DisplayName,
    string? Bio,
    string? CountryCode,
    ProfileVisibility Visibility) : ICommand;
