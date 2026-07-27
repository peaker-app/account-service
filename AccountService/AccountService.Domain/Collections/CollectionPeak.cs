namespace AccountService.Domain.Collections;

public sealed class CollectionPeak
{
    private CollectionPeak()
    {
    }

    private CollectionPeak(Guid id, CollectionPeakSnapshot peak, DateTime addedAtUtc)
    {
        Id = id;
        PeakId = peak.PeakId;
        PeakName = peak.Name;
        PeakAltitudeMeters = peak.AltitudeMeters;
        AddedAtUtc = addedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid PeakId { get; private set; }

    public string PeakName { get; private set; } = null!;

    public int PeakAltitudeMeters { get; private set; }

    public DateTime AddedAtUtc { get; private set; }

    internal static CollectionPeak Create(CollectionPeakSnapshot peak, DateTime addedAtUtc) =>
        new(Guid.CreateVersion7(), peak, addedAtUtc);

    internal bool Sync(CollectionPeakSnapshot peak)
    {
        if (string.Equals(PeakName, peak.Name, StringComparison.Ordinal)
            && PeakAltitudeMeters == peak.AltitudeMeters)
        {
            return false;
        }

        PeakName = peak.Name;
        PeakAltitudeMeters = peak.AltitudeMeters;

        return true;
    }
}
