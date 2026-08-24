namespace AccountService.Domain.Profiles;

public sealed record ProfileStatsUpdate(ProfileStatsSnapshot Overall, ProfileStatsSnapshot Public)
{
    public static readonly ProfileStatsUpdate Empty =
        new(ProfileStatsSnapshot.Empty, ProfileStatsSnapshot.Empty);
}
