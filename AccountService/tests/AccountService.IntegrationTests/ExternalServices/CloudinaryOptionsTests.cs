using System.ComponentModel.DataAnnotations;
using AccountService.Infrastructure.ExternalServices;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.ExternalServices;

public sealed class CloudinaryOptionsTests
{
    [Fact]
    public void Validation_AcceptsAnEmptyAuthTokenKey()
    {
        CloudinaryOptions options = new()
        {
            CloudName = "peaker-test",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            AuthTokenKey = string.Empty
        };

        Validator.TryValidateObject(options, new ValidationContext(options), null, validateAllProperties: true)
            .Should().BeTrue();
    }
}
