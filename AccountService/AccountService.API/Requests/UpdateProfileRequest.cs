using System.Text.Json.Serialization;
using AccountService.Application.Profiles.UpdateProfile;
using AccountService.Domain.Profiles;

namespace AccountService.API.Requests;

public sealed record UpdateProfileRequest(
    string DisplayName,
    string? Bio,
    string? CountryCode,
    [property: JsonRequired] ProfileVisibility Visibility)
{
    public UpdateProfileCommand ToCommand(Guid userId) =>
        new(userId, DisplayName, Bio, CountryCode, Visibility);
}
