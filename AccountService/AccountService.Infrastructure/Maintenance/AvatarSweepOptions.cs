namespace AccountService.Infrastructure.Maintenance;

public sealed class AvatarSweepOptions
{
    public const string SectionName = "AvatarSweep";

    public bool Enabled { get; init; } = true;

    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan Retention { get; init; } = TimeSpan.FromHours(24);
}
