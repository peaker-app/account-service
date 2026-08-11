namespace AccountService.Application.Abstractions;

public sealed record StoredAsset(string PublicId, DateTime CreatedAtUtc);

public interface IAvatarAssetInventory
{
    Task<IReadOnlyCollection<StoredAsset>> ListQuarantinedAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StoredAsset>> ListConfirmedAsync(CancellationToken cancellationToken);
}
