using System.Collections.Concurrent;
using AccountService.Application.Abstractions;
using Common.Domain.Results;

namespace AccountService.IntegrationTests.Fakes;

internal sealed class FakeImageStorage : IImageStorage
{
    private int _counter;

    public ConcurrentBag<string> DeletedPublicIds { get; } = [];

    public ConcurrentBag<string> ConfirmedPublicIds { get; } = [];

    public Task<Result<StoredImage>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken)
    {
        int index = Interlocked.Increment(ref _counter);
        StoredImage stored = new($"peaker/test/avatars/avatar-{index}");

        return Task.FromResult<Result<StoredImage>>(stored);
    }

    public Task ConfirmAsync(string publicId, CancellationToken cancellationToken)
    {
        ConfirmedPublicIds.Add(publicId);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        DeletedPublicIds.Add(publicId);

        return Task.CompletedTask;
    }

    public async Task<bool> TryDeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        await DeleteAsync(publicId, cancellationToken);

        return true;
    }
}
