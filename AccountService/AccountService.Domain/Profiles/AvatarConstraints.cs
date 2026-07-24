using System.Collections.Frozen;

namespace AccountService.Domain.Profiles;

public static class AvatarConstraints
{
    public const long MaxSizeBytes = 5L * 1024 * 1024;

    public static readonly FrozenSet<string> AllowedContentTypes =
        new[] { "image/jpeg", "image/png", "image/webp" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsAllowedContentType(string? contentType) =>
        contentType is not null && AllowedContentTypes.Contains(contentType);
}
