using AccountService.Domain.ProfileAscents;
using AccountService.Domain.UnitTests.TestData;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.ProfileAscents;

public sealed class PeakSnapshotTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSnapshot()
    {
        Result<PeakSnapshot> result = PeakSnapshot.Create(ProfileAscentMother.AnetoId, "Aneto", 3404);

        result.Value.Name.Should().Be("Aneto");
    }

    [Fact]
    public void Create_WithoutPeakId_ReturnsPeakRequired()
    {
        Result<PeakSnapshot> result = PeakSnapshot.Create(Guid.Empty, "Aneto", 3404);

        result.Error.Should().Be(ProfileAscentErrors.PeakRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutName_ReturnsPeakNameRequired(string? name)
    {
        Result<PeakSnapshot> result = PeakSnapshot.Create(ProfileAscentMother.AnetoId, name, 3404);

        result.Error.Should().Be(ProfileAscentErrors.PeakNameRequired);
    }

    [Fact]
    public void Create_WithNameLongerThanTheLimit_ReturnsPeakNameTooLong()
    {
        string name = new('A', PeakSnapshot.MaxNameLength + 1);

        Result<PeakSnapshot> result = PeakSnapshot.Create(ProfileAscentMother.AnetoId, name, 3404);

        result.Error.Should().Be(ProfileAscentErrors.PeakNameTooLong);
    }

    [Fact]
    public void Create_WithNegativeAltitude_IsAccepted()
    {
        Result<PeakSnapshot> result = PeakSnapshot.Create(ProfileAscentMother.AnetoId, "Depresión", -155);

        result.IsSuccess.Should().BeTrue();
    }
}
