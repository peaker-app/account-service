using AccountService.Domain.Collections;
using AccountService.Domain.UnitTests.TestData;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Collections;

public sealed class CollectionTests
{
    [Fact]
    public void CreateDefault_ForAProfile_IsMarkedAsTheDefaultOne()
    {
        Collection collection = Collection.CreateDefault(CollectionMother.ProfileId).Value;

        collection.Should().BeEquivalentTo(new
        {
            ProfileId = CollectionMother.ProfileId,
            Kind = CollectionKind.WantToClimb,
            IsDefault = true,
            PeakCount = 0
        });
    }

    [Fact]
    public void CreateDefault_ForAProfile_StoresTheEnglishName()
    {
        Collection collection = CollectionMother.Default();

        collection.Name.Value.Should().Be(Collection.DefaultName);
    }

    [Fact]
    public void Create_WithValidDraft_IsCustom()
    {
        Result<Collection> result = Collection.Create(CollectionMother.Draft());

        result.Value.Kind.Should().Be(CollectionKind.Custom);
    }

    [Fact]
    public void Create_WithDescriptionLongerThanTheMaximum_ReturnsDescriptionTooLong()
    {
        CollectionDraft draft = CollectionMother.Draft(
            description: new string('a', Collection.MaxDescriptionLength + 1));

        Result<Collection> result = Collection.Create(draft);

        result.Error.Should().Be(CollectionErrors.DescriptionTooLong);
    }

    [Fact]
    public void UpdateDetails_OnACustomCollection_ReplacesNameAndDescription()
    {
        Collection collection = CollectionMother.Custom();

        Result result = collection.UpdateDetails(
            new CollectionDetails(CollectionMother.Name("Alpes"), "Los cuatromiles."));

        result.IsSuccess.Should().BeTrue();
        collection.Should().BeEquivalentTo(new { Description = "Los cuatromiles." });
        collection.Name.Value.Should().Be("Alpes");
    }

    [Fact]
    public void UpdateDetails_OnTheDefaultCollection_ReturnsDefaultNotEditable()
    {
        Collection collection = CollectionMother.Default();

        Result result = collection.UpdateDetails(new CollectionDetails(CollectionMother.Name("Otra"), null));

        result.Error.Should().Be(CollectionErrors.DefaultNotEditable);
    }

    [Fact]
    public void UpdateDetails_OnTheDefaultCollection_LeavesTheNameUntouched()
    {
        Collection collection = CollectionMother.Default();

        collection.UpdateDetails(new CollectionDetails(CollectionMother.Name("Otra"), null));

        collection.Name.Value.Should().Be(Collection.DefaultName);
    }

    [Fact]
    public void UpdateDetails_WithDescriptionLongerThanTheMaximum_ReturnsDescriptionTooLong()
    {
        Collection collection = CollectionMother.Custom();
        CollectionDetails details = new(
            CollectionMother.Name("Alpes"),
            new string('a', Collection.MaxDescriptionLength + 1));

        Result result = collection.UpdateDetails(details);

        result.Error.Should().Be(CollectionErrors.DescriptionTooLong);
    }

    [Fact]
    public void EnsureDeletable_OnACustomCollection_Succeeds()
    {
        Collection collection = CollectionMother.Custom();

        collection.EnsureDeletable().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void EnsureDeletable_OnTheDefaultCollection_ReturnsDefaultNotDeletable()
    {
        Collection collection = CollectionMother.Default();

        collection.EnsureDeletable().Error.Should().Be(CollectionErrors.DefaultNotDeletable);
    }

    [Fact]
    public void AddPeak_WithANewPeak_KeepsTheDenormalisedNameAndAltitude()
    {
        Collection collection = CollectionMother.Custom();

        Result<CollectionPeak> result = collection.AddPeak(CollectionMother.Aneto, CollectionMother.AddedAtUtc);

        result.Value.Should().BeEquivalentTo(new
        {
            PeakId = CollectionMother.AnetoId,
            PeakName = "Aneto",
            PeakAltitudeMeters = 3404,
            AddedAtUtc = CollectionMother.AddedAtUtc
        });
    }

    [Fact]
    public void AddPeak_WithAPeakAlreadyInTheCollection_ReturnsPeakAlreadyAdded()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        Result<CollectionPeak> result = collection.AddPeak(CollectionMother.Aneto, CollectionMother.AddedAtUtc);

        result.Error.Should().Be(CollectionErrors.PeakAlreadyAdded);
    }

    [Fact]
    public void AddPeak_WithAPeakAlreadyInTheCollection_DoesNotDuplicateIt()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        collection.AddPeak(CollectionMother.Aneto, CollectionMother.AddedAtUtc);

        collection.PeakCount.Should().Be(1);
    }

    [Fact]
    public void AddPeak_OnTheDefaultCollection_IsAllowed()
    {
        Collection collection = CollectionMother.Default();

        Result<CollectionPeak> result = collection.AddPeak(CollectionMother.Aneto, CollectionMother.AddedAtUtc);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemovePeak_WithAPeakInTheCollection_DropsIt()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto, CollectionMother.MontBlanc);

        collection.RemovePeak(CollectionMother.AnetoId);

        collection.Peaks.Should().OnlyContain(peak => peak.PeakId == CollectionMother.MontBlancId);
    }

    [Fact]
    public void RemovePeak_WithAPeakOutsideTheCollection_ReturnsPeakNotInCollection()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        Result result = collection.RemovePeak(CollectionMother.MontBlancId);

        result.Error.Should().Be(CollectionErrors.PeakNotInCollection(CollectionMother.MontBlancId));
    }

    [Fact]
    public void SyncPeak_WithARenamedPeak_UpdatesTheDenormalisedCopy()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        collection.SyncPeak(CollectionMother.Peak(CollectionMother.AnetoId, "Pico de Aneto", 3410));

        collection.Peaks.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { PeakName = "Pico de Aneto", PeakAltitudeMeters = 3410 });
    }

    [Fact]
    public void SyncPeak_WithUnchangedData_ReportsNoChange()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        collection.SyncPeak(CollectionMother.Aneto).Should().BeFalse();
    }

    [Fact]
    public void SyncPeak_WithAPeakOutsideTheCollection_LeavesEveryPeakUntouched()
    {
        Collection collection = CollectionMother.WithPeaks(CollectionMother.Aneto);

        collection.SyncPeak(CollectionMother.Peak(CollectionMother.MontBlancId, "Monte Bianco", 4810));

        collection.Peaks.Should().ContainSingle().Which.PeakName.Should().Be("Aneto");
    }

    [Fact]
    public void IsOwnedBy_WithAnotherProfile_ReturnsFalse()
    {
        Collection collection = CollectionMother.Custom();

        collection.IsOwnedBy(Guid.CreateVersion7()).Should().BeFalse();
    }
}
