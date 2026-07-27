using AccountService.Domain.Collections;

namespace AccountService.Domain.UnitTests.TestData;

internal static class CollectionMother
{
    public static readonly Guid ProfileId = Guid.CreateVersion7();

    public static readonly Guid AnetoId = Guid.CreateVersion7();

    public static readonly Guid MontBlancId = Guid.CreateVersion7();

    public static readonly DateTime AddedAtUtc = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    public static CollectionPeakSnapshot Aneto => Peak(AnetoId, "Aneto", 3404);

    public static CollectionPeakSnapshot MontBlanc => Peak(MontBlancId, "Mont Blanc", 4808);

    public static CollectionPeakSnapshot Peak(Guid peakId, string name, int altitudeMeters) =>
        CollectionPeakSnapshot.Create(peakId, name, altitudeMeters).Value;

    public static CollectionName Name(string value = "Tresmiles del Pirineo") =>
        CollectionName.Create(value).Value;

    public static CollectionDraft Draft(string name = "Tresmiles del Pirineo", string? description = null) =>
        new(ProfileId, new CollectionDetails(Name(name), description));

    public static Collection Custom(string name = "Tresmiles del Pirineo", string? description = null) =>
        Collection.Create(Draft(name, description)).Value;

    public static Collection Default() => Collection.CreateDefault(ProfileId).Value;

    public static Collection WithPeaks(params CollectionPeakSnapshot[] peaks)
    {
        Collection collection = Custom();

        foreach (CollectionPeakSnapshot peak in peaks)
        {
            collection.AddPeak(peak, AddedAtUtc);
        }

        return collection;
    }
}
