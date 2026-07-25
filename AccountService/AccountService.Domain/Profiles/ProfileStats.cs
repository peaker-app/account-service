namespace AccountService.Domain.Profiles;

public sealed class ProfileStats
{
    private ProfileStats()
    {
    }

    private ProfileStats(ProfileStatsUpdate update, DateTime updatedAtUtc) => Apply(update, updatedAtUtc);

    public int TotalAscents { get; private set; }

    public int DistinctPeaks { get; private set; }

    public int HighestAltitudeMeters { get; private set; }

    public Guid? HighestPeakId { get; private set; }

    public string? HighestPeakName { get; private set; }

    public DateOnly? LastAscentDate { get; private set; }

    public int PublicTotalAscents { get; private set; }

    public int PublicDistinctPeaks { get; private set; }

    public int PublicHighestAltitudeMeters { get; private set; }

    public Guid? PublicHighestPeakId { get; private set; }

    public string? PublicHighestPeakName { get; private set; }

    public DateOnly? PublicLastAscentDate { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public ProfileStatsSnapshot Overall => new(
        TotalAscents,
        DistinctPeaks,
        HighestAltitudeMeters,
        HighestPeakId,
        HighestPeakName,
        LastAscentDate);

    public ProfileStatsSnapshot Public => new(
        PublicTotalAscents,
        PublicDistinctPeaks,
        PublicHighestAltitudeMeters,
        PublicHighestPeakId,
        PublicHighestPeakName,
        PublicLastAscentDate);

    public static ProfileStats Initial(DateTime utcNow) => new(ProfileStatsUpdate.Empty, utcNow);

    internal void Apply(ProfileStatsUpdate update, DateTime utcNow)
    {
        ApplyOverall(update.Overall);
        ApplyPublic(update.Public);
        UpdatedAtUtc = utcNow;
    }

    private void ApplyOverall(ProfileStatsSnapshot snapshot)
    {
        TotalAscents = snapshot.TotalAscents;
        DistinctPeaks = snapshot.DistinctPeaks;
        HighestAltitudeMeters = snapshot.HighestAltitudeMeters;
        HighestPeakId = snapshot.HighestPeakId;
        HighestPeakName = snapshot.HighestPeakName;
        LastAscentDate = snapshot.LastAscentDate;
    }

    private void ApplyPublic(ProfileStatsSnapshot snapshot)
    {
        PublicTotalAscents = snapshot.TotalAscents;
        PublicDistinctPeaks = snapshot.DistinctPeaks;
        PublicHighestAltitudeMeters = snapshot.HighestAltitudeMeters;
        PublicHighestPeakId = snapshot.HighestPeakId;
        PublicHighestPeakName = snapshot.HighestPeakName;
        PublicLastAscentDate = snapshot.LastAscentDate;
    }
}
