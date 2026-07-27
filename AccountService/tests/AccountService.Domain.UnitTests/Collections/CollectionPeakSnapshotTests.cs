using AccountService.Domain.Collections;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Collections;

public sealed class CollectionPeakSnapshotTests
{
    [Fact]
    public void Create_WithoutPeakId_ReturnsPeakRequired()
    {
        Result<CollectionPeakSnapshot> result = CollectionPeakSnapshot.Create(Guid.Empty, "Aneto", 3404);

        result.Error.Should().Be(CollectionErrors.PeakRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ReturnsPeakNameRequired(string? name)
    {
        Result<CollectionPeakSnapshot> result =
            CollectionPeakSnapshot.Create(Guid.CreateVersion7(), name, 3404);

        result.Error.Should().Be(CollectionErrors.PeakNameRequired);
    }

    [Fact]
    public void Create_WithNameLongerThanTheMaximum_ReturnsPeakNameTooLong()
    {
        Result<CollectionPeakSnapshot> result = CollectionPeakSnapshot.Create(
            Guid.CreateVersion7(),
            new string('a', CollectionPeakSnapshot.MaxNameLength + 1),
            3404);

        result.Error.Should().Be(CollectionErrors.PeakNameTooLong);
    }

    [Fact]
    public void Create_WithValidData_TrimsTheName()
    {
        Result<CollectionPeakSnapshot> result =
            CollectionPeakSnapshot.Create(Guid.CreateVersion7(), "  Aneto  ", 3404);

        result.Value.Name.Should().Be("Aneto");
    }
}
