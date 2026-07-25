using Common.Domain.Results;

namespace AccountService.Domain.ProfileAscents;

public static class AscentVisibilities
{
    public static Result<AscentVisibility> Parse(string? value) =>
        Enum.TryParse(value, ignoreCase: true, out AscentVisibility visibility) && Enum.IsDefined(visibility)
            ? visibility
            : ProfileAscentErrors.VisibilityInvalid;
}
