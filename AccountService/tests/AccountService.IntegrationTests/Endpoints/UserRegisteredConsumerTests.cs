using Common.Contracts.Users;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class UserRegisteredConsumerTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task Consume_UserRegistered_CreatesProfileReactively()
    {
        Guid userId = ApiTestHelpers.NewUserId();

        await _factory.PublishUserRegisteredAsync(NewMessage(userId));

        bool created = await _factory.WaitForProfileAsync(userId);
        created.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_UserRegistered_CreatesTheDefaultCollection()
    {
        Guid userId = ApiTestHelpers.NewUserId();

        await _factory.PublishUserRegisteredAsync(NewMessage(userId));

        bool created = await _factory.WaitForDefaultCollectionAsync(userId);
        created.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_SameMessageTwice_CreatesSingleProfile()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        UserRegistered message = NewMessage(userId);

        await _factory.PublishUserRegisteredAsync(message);
        await _factory.WaitForProfileAsync(userId);
        await _factory.PublishUserRegisteredAsync(message);
        await Task.Delay(TimeSpan.FromSeconds(2));

        int count = await _factory.CountProfilesAsync(userId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Consume_SameMessageTwice_CreatesSingleDefaultCollection()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        UserRegistered message = NewMessage(userId);

        await _factory.PublishUserRegisteredAsync(message);
        await _factory.WaitForDefaultCollectionAsync(userId);
        await _factory.PublishUserRegisteredAsync(message);
        await Task.Delay(TimeSpan.FromSeconds(2));

        int count = await _factory.CountDefaultCollectionsAsync(userId);
        count.Should().Be(1);
    }

    private static UserRegistered NewMessage(Guid userId) => new()
    {
        MessageId = Guid.CreateVersion7(),
        UserId = userId,
        Email = $"user-{userId:N}@peaker.io",
        Username = ApiTestHelpers.UniqueUsername(),
        OccurredAtUtc = DateTime.UtcNow
    };
}
