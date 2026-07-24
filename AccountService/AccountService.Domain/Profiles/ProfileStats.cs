namespace AccountService.Domain.Profiles;

public sealed class ProfileStats
{
    private ProfileStats()
    {
    }

    private ProfileStats(DateTime updatedAtUtc) => UpdatedAtUtc = updatedAtUtc;

    public int TotalAscents { get; private set; }

    public int DistinctPeaks { get; private set; }

    public int HighestAltitudeMeters { get; private set; }

    public Guid? HighestPeakId { get; private set; }

    public string? HighestPeakName { get; private set; }

    public DateOnly? LastAscentDate { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static ProfileStats Initial(DateTime utcNow) => new(utcNow);
}
