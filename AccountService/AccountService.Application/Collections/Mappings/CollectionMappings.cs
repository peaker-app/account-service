using AccountService.Application.Collections.GetCollectionById;
using AccountService.Domain.Collections;

namespace AccountService.Application.Collections.Mappings;

internal static class CollectionMappings
{
    public static CollectionPeakResponse ToResponse(this CollectionPeak peak) => new(
        peak.Id,
        peak.PeakId,
        peak.PeakName,
        peak.PeakAltitudeMeters,
        peak.AddedAtUtc);
}
