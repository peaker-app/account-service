using System.Collections.Concurrent;
using AccountService.Application.Abstractions;

namespace AccountService.IntegrationTests.Fakes;

internal sealed class FakeAvatarAssetInventory : IAvatarAssetInventory
{
    public ConcurrentBag<StoredAsset> Quarantined { get; } = [];

    public ConcurrentBag<StoredAsset> Confirmed { get; } = [];

    public void Clear()
    {
        Quarantined.Clear();
        Confirmed.Clear();
    }

    public Task<IReadOnlyCollection<StoredAsset>> ListQuarantinedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<StoredAsset>>([.. Quarantined]);

    public Task<IReadOnlyCollection<StoredAsset>> ListConfirmedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<StoredAsset>>([.. Confirmed]);
}
