using Common.Domain.Results;

namespace AccountService.Domain.ProfileAscents;

public sealed record PeakSnapshot
{
    public const int MaxNameLength = 200;

    private PeakSnapshot(Guid peakId, string name, int altitudeMeters)
    {
        PeakId = peakId;
        Name = name;
        AltitudeMeters = altitudeMeters;
    }

    public Guid PeakId { get; }

    public string Name { get; }

    public int AltitudeMeters { get; }

    public static Result<PeakSnapshot> Create(Guid peakId, string? name, int altitudeMeters)
    {
        if (peakId == Guid.Empty)
        {
            return ProfileAscentErrors.PeakRequired;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return ProfileAscentErrors.PeakNameRequired;
        }

        string trimmed = name.Trim();

        return trimmed.Length > MaxNameLength
            ? ProfileAscentErrors.PeakNameTooLong
            : new PeakSnapshot(peakId, trimmed, altitudeMeters);
    }
}
