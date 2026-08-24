using Common.Contracts.Users;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class UserDeletedConsumerTests(AccountServiceApiFactory factory)
{
    private static readonly byte[] PngContent = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01];

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
    public async Task Consume_UserDeleted_RemovesTheAvatarFromRemoteStorage()
    {
        Guid userId = await SeededProfileAsync();
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);
        (await client.UploadAvatarAsync(PngContent, "image/png", "avatar.png")).Dispose();

        await _factory.PublishUserDeletedAsync(NewMessage(userId));
        await _factory.WaitForProfileRemovalAsync(userId);

        bool deleted = await WaitForAvatarDeletionAsync();
        deleted.Should().BeTrue();
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

    private async Task<bool> WaitForAvatarDeletionAsync()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (!_factory.ImageStorage.DeletedPublicIds.IsEmpty)
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return false;
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
