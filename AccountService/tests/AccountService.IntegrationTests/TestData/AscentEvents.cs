using Common.Contracts.Ascents;
using Common.Contracts.Peaks;

namespace AccountService.IntegrationTests.TestData;

internal static class AscentEvents
{
    public const string Public = "Public";

    public const string Private = "Private";

    public static AscentRegistered Registered(
        Guid userId,
        Guid ascentId,
        Guid peakId,
        int altitudeMeters,
        string visibility = Public,
        string peakName = "Aneto",
        DateOnly? ascentDate = null) => new()
        {
            AscentId = ascentId,
            UserId = userId,
            PeakId = peakId,
            PeakName = peakName,
            PeakAltitudeM = altitudeMeters,
            AscentDate = ascentDate ?? new DateOnly(2025, 7, 14),
            Visibility = visibility,
            OccurredAtUtc = DateTime.UtcNow
        };

    public static AscentUpdated Updated(
        Guid userId,
        Guid ascentId,
        Guid peakId,
        DateOnly ascentDate,
        string visibility = Public) => new()
        {
            AscentId = ascentId,
            UserId = userId,
            PeakId = peakId,
            AscentDate = ascentDate,
            Visibility = visibility,
            OccurredAtUtc = DateTime.UtcNow
        };

    public static AscentDeleted Deleted(Guid userId, Guid ascentId, Guid peakId) => new()
    {
        AscentId = ascentId,
        UserId = userId,
        PeakId = peakId,
        AscentDate = new DateOnly(2025, 7, 14),
        OccurredAtUtc = DateTime.UtcNow
    };

    public static PeakRenamed Renamed(Guid peakId, string name, int altitudeMeters) => new()
    {
        PeakId = peakId,
        Name = name,
        AltitudeM = altitudeMeters,
        CountryCode = "ES",
        OccurredAtUtc = DateTime.UtcNow
    };
}
