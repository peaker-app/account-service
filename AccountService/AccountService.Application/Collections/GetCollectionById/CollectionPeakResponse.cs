namespace AccountService.Application.Collections.GetCollectionById;

public sealed record CollectionPeakResponse(
    Guid Id,
    Guid PeakId,
    string PeakName,
    int PeakAltitudeMeters,
    DateTime AddedAtUtc);
