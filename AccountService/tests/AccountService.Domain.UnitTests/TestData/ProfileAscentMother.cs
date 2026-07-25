using AccountService.Domain.ProfileAscents;

namespace AccountService.Domain.UnitTests.TestData;

internal static class ProfileAscentMother
{
    public static readonly Guid ProfileId = Guid.CreateVersion7();

    public static readonly Guid AnetoId = Guid.CreateVersion7();

    public static readonly Guid MontBlancId = Guid.CreateVersion7();

    public static PeakSnapshot Aneto => Peak(AnetoId, "Aneto", 3404);

    public static PeakSnapshot MontBlanc => Peak(MontBlancId, "Mont Blanc", 4808);

    public static PeakSnapshot Peak(Guid peakId, string name, int altitudeMeters) =>
        PeakSnapshot.Create(peakId, name, altitudeMeters).Value;

    public static ProfileAscentDraft Draft(
        PeakSnapshot? peak = null,
        DateOnly? ascentDate = null,
        AscentVisibility visibility = AscentVisibility.Public) =>
        new(
            ProfileId,
            Guid.CreateVersion7(),
            peak ?? Aneto,
            ascentDate ?? new DateOnly(2025, 7, 14),
            visibility);

    public static ProfileAscent Create(
        PeakSnapshot? peak = null,
        DateOnly? ascentDate = null,
        AscentVisibility visibility = AscentVisibility.Public) =>
        ProfileAscent.Create(Draft(peak, ascentDate, visibility)).Value;
}
