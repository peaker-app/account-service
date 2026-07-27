using Common.Contracts.Users;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class UserDeletedConsumerTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task Consume_UserDeleted_RemovesTheProfile()
    {
        Guid userId = await SeededProfileAsync();

        await _factory.PublishUserDeletedAsync(NewMessage(userId));

        bool removed = await _factory.WaitForProfileRemovalAsync(userId);
        removed.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_SameMessageTwice_LeavesTheProfileRemoved()
    {
        Guid userId = await SeededProfileAsync();
        UserDeleted message = NewMessage(userId);

        await _factory.PublishUserDeletedAsync(message);
        await _factory.WaitForProfileRemovalAsync(userId);
        await _factory.PublishUserDeletedAsync(message);
        await Task.Delay(TimeSpan.FromSeconds(2));

        int count = await _factory.CountProfilesAsync(userId);
        count.Should().Be(0);
    }

    [Fact]
    public async Task Consume_ForAnUnknownUser_IsIgnored()
    {
        Guid known = await SeededProfileAsync();

        await _factory.PublishUserDeletedAsync(NewMessage(ApiTestHelpers.NewUserId()));
        await Task.Delay(TimeSpan.FromSeconds(2));

        int survivors = await _factory.CountProfilesAsync(known);
        survivors.Should().Be(1);
    }

    private async Task<Guid> SeededProfileAsync()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());

        return userId;
    }

    private static UserDeleted NewMessage(Guid userId) => new()
    {
        MessageId = Guid.CreateVersion7(),
        UserId = userId,
        OccurredAtUtc = DateTime.UtcNow
    };
}
