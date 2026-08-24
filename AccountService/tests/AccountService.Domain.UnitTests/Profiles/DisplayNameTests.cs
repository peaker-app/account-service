using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Profiles;

public sealed class DisplayNameTests
{
    [Fact]
    public void Create_WithValidName_TrimsAndSucceeds()
    {
        Result<DisplayName> result = DisplayName.Create("  Rubén  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Rubén");
    }

    [Fact]
    public void Create_WithNullOrWhitespace_ReturnsDisplayNameEmpty()
    {
        Result<DisplayName> result = DisplayName.Create("   ");

        result.Error.Should().Be(ProfileErrors.DisplayNameEmpty);
    }

    [Fact]
    public void Create_ExceedingMaxLength_ReturnsDisplayNameTooLong()
    {
        Result<DisplayName> result = DisplayName.Create(new string('a', DisplayName.MaxLength + 1));

        result.Error.Should().Be(ProfileErrors.DisplayNameTooLong);
    }

    [Fact]
    public void Create_AtMaxLength_Succeeds()
    {
        Result<DisplayName> result = DisplayName.Create(new string('a', DisplayName.MaxLength));

        result.IsSuccess.Should().BeTrue();
    }
}
