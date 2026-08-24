using AccountService.Domain.Collections;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Collections;

public sealed class CollectionNameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankValue_ReturnsNameRequired(string? value)
    {
        Result<CollectionName> result = CollectionName.Create(value);

        result.Error.Should().Be(CollectionErrors.NameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanTheMaximum_ReturnsNameTooLong()
    {
        Result<CollectionName> result = CollectionName.Create(new string('a', CollectionName.MaxLength + 1));

        result.Error.Should().Be(CollectionErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsTheValue()
    {
        Result<CollectionName> result = CollectionName.Create("  Alpes  ");

        result.Value.Value.Should().Be("Alpes");
    }

    [Fact]
    public void Equals_WithTheSameValue_ReturnsTrue()
    {
        CollectionName name = CollectionName.Create("Alpes").Value;

        name.Should().Be(CollectionName.Create("Alpes").Value);
    }
}
