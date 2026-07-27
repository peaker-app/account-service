using AccountService.Domain.Collections;

namespace AccountService.Application.UnitTests.TestData;

internal static class CollectionFactory
{
    public static readonly DateTime Now = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    public static readonly Guid ProfileId = Guid.CreateVersion7();

    public static CollectionPeakSnapshot Peak(Guid peakId, string name = "Aneto", int altitudeMeters = 3404) =>
        CollectionPeakSnapshot.Create(peakId, name, altitudeMeters).Value;

    public static Collection Custom(string name = "Tresmiles del Pirineo") =>
        Collection.Create(
            new CollectionDraft(ProfileId, new CollectionDetails(CollectionName.Create(name).Value, null))).Value;

    public static Collection Default() => Collection.CreateDefault(ProfileId).Value;

    public static Collection WithPeak(Guid peakId)
    {
        Collection collection = Custom();
        collection.AddPeak(Peak(peakId), Now);

        return collection;
    }
}
