using Common.Domain.Results;

namespace AccountService.Domain.Collections;

public sealed record CollectionPeakSnapshot
{
    public const int MaxNameLength = 200;

    private CollectionPeakSnapshot(Guid peakId, string name, int altitudeMeters)
    {
        PeakId = peakId;
        Name = name;
        AltitudeMeters = altitudeMeters;
    }

    public Guid PeakId { get; }

    public string Name { get; }

    public int AltitudeMeters { get; }

    public static Result<CollectionPeakSnapshot> Create(Guid peakId, string? name, int altitudeMeters)
    {
        if (peakId == Guid.Empty)
        {
            return CollectionErrors.PeakRequired;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return CollectionErrors.PeakNameRequired;
        }

        string trimmed = name.Trim();

        return trimmed.Length > MaxNameLength
            ? CollectionErrors.PeakNameTooLong
            : new CollectionPeakSnapshot(peakId, trimmed, altitudeMeters);
    }
}
