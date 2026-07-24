using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Profiles;

public sealed class ProfileSlugTests
{
    [Theory]
    [InlineData("ruben")]
    [InlineData("ruben-fer")]
    [InlineData("r2d2")]
    public void Create_WithValidKebabCase_Succeeds(string raw)
    {
        Result<ProfileSlug> result = ProfileSlug.Create(raw);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("Ruben")]
    [InlineData("ruben_fer")]
    [InlineData("-ruben")]
    [InlineData("ruben-")]
    [InlineData("ruben--fer")]
    [InlineData("rubén")]
    public void Create_WithInvalidFormat_ReturnsSlugInvalid(string raw)
    {
        Result<ProfileSlug> result = ProfileSlug.Create(raw);

        result.Error.Should().Be(ProfileErrors.SlugInvalid);
    }

    [Theory]
    [InlineData("Hiker_Ruben", "hiker-ruben")]
    [InlineData("ruben.fernandez", "ruben-fernandez")]
    [InlineData("RUBEN", "ruben")]
    [InlineData("rubén", "ruben")]
    public void FromUsername_NormalizesToKebabCase(string username, string expected)
    {
        ProfileSlug slug = ProfileSlug.FromUsername(username);

        slug.Value.Should().Be(expected);
    }

    [Fact]
    public void FromUsername_ProducesAValidSlug()
    {
        ProfileSlug slug = ProfileSlug.FromUsername("Hiker_Ruben.2");

        ProfileSlug.Create(slug.Value).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void WithSuffix_AppendsTheNumberKeepingKebabCase()
    {
        ProfileSlug slug = ProfileSlug.FromUsername("ruben");

        ProfileSlug suffixed = slug.WithSuffix(2);

        suffixed.Value.Should().Be("ruben-2");
        ProfileSlug.Create(suffixed.Value).IsSuccess.Should().BeTrue();
    }
}
