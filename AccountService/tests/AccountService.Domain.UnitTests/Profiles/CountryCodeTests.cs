using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Profiles;

public sealed class CountryCodeTests
{
    [Theory]
    [InlineData("ES", "ES")]
    [InlineData("es", "ES")]
    [InlineData("  fr  ", "FR")]
    public void Create_WithValidIsoCode_NormalizesToUppercase(string raw, string expected)
    {
        Result<CountryCode> result = CountryCode.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("España")]
    [InlineData("E")]
    [InlineData("   ")]
    public void Create_WithInvalidCode_ReturnsCountryCodeInvalid(string raw)
    {
        Result<CountryCode> result = CountryCode.Create(raw);

        result.Error.Should().Be(ProfileErrors.CountryCodeInvalid);
    }
}
