namespace AccountService.Domain.Profiles;

public static class AvatarConstraints
{
    public const long MaxSizeBytes = 5L * 1024 * 1024;

    public const int MaxRequestSizeBytes = (int)MaxSizeBytes + (64 * 1024);
}
