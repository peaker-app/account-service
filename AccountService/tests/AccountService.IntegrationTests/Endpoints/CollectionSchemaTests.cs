using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class CollectionSchemaTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task Insert_ASecondDefaultCollectionUnderAnotherName_IsRejectedByTheDatabase()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());

        Exception? failure = await _factory.TryInsertDefaultCollectionAsync(userId, "Quiero subir");

        failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Insert_ADefaultCollectionForAProfileWithoutOne_Succeeds()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        await _factory.TryInsertDefaultCollectionAsync(userId, "Duplicada");

        int count = await _factory.CountDefaultCollectionsAsync(userId);

        count.Should().Be(1);
    }
}
