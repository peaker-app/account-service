using AccountService.Domain.ProfileAscents;

namespace AccountService.Application.UnitTests.TestData;

internal static class ProfileAscentFactory
{
    public static readonly Guid AnetoId = Guid.CreateVersion7();

    public static readonly Guid MontBlancId = Guid.CreateVersion7();

    public static PeakSnapshot Aneto => Peak(AnetoId, "Aneto", 3404);

    public static PeakSnapshot MontBlanc => Peak(MontBlancId, "Mont Blanc", 4808);

    public static PeakSnapshot Peak(Guid peakId, string name, int altitudeMeters) =>
        PeakSnapshot.Create(peakId, name, altitudeMeters).Value;

    public static ProfileAscent For(
        Guid profileId,
        Guid? ascentId = null,
        PeakSnapshot? peak = null,
        DateOnly? ascentDate = null,
        AscentVisibility visibility = AscentVisibility.Public) =>
        ProfileAscent.Create(new ProfileAscentDraft(
            profileId,
            ascentId ?? Guid.CreateVersion7(),
            peak ?? Aneto,
            ascentDate ?? new DateOnly(2025, 7, 14),
            visibility)).Value;
}
