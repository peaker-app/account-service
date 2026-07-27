namespace AccountService.Domain.Collections;

public interface ICollectionRepository
{
    Task<Collection?> GetOwnedByUserAsync(Guid collectionId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Collection>> GetByPeakAsync(Guid peakId, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(CollectionNameLookup lookup, CancellationToken cancellationToken);

    void Add(Collection collection);

    void Remove(Collection collection);
}
