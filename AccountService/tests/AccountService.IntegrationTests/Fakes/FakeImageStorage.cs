using System.Collections.Concurrent;
using AccountService.Application.Abstractions;
using Common.Domain.Results;

namespace AccountService.IntegrationTests.Fakes;

internal sealed class FakeImageStorage : IImageStorage
{
    private int _counter;

    public ConcurrentBag<string> DeletedPublicIds { get; } = [];

    public Task<Result<StoredImage>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken)
    {
        int index = Interlocked.Increment(ref _counter);
        var stored = new StoredImage($"peaker/test/avatars/avatar-{index}", $"https://cdn.test/avatar-{index}.png");

        return Task.FromResult<Result<StoredImage>>(stored);
    }

    public Task DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        DeletedPublicIds.Add(publicId);

        return Task.CompletedTask;
    }
}
