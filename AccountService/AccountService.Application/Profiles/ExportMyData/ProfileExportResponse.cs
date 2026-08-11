using AccountService.Application.Profiles.GetMyProfile;
using AccountService.Application.Profiles.GetMyStats;

namespace AccountService.Application.Profiles.ExportMyData;

public sealed record ExportedCollectionPeakResponse(
    Guid PeakId,
    string PeakName,
    int PeakAltitudeMeters,
    DateTime AddedAtUtc);

public sealed record ExportedCollectionResponse(
    Guid Id,
    string Name,
    string? Description,
    string Kind,
    IReadOnlyList<ExportedCollectionPeakResponse> Peaks);

public sealed record ProfileExportResponse(
    ProfileResponse Profile,
    ProfileStatsResponse Stats,
    IReadOnlyList<ExportedCollectionResponse> Collections);
