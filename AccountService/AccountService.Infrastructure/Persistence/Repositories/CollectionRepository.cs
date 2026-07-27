using AccountService.Domain.Collections;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

internal sealed class CollectionRepository(AccountDbContext context) : ICollectionRepository
{
    public Task<Collection?> GetOwnedByUserAsync(
        Guid collectionId,
        Guid userId,
        CancellationToken cancellationToken) =>
        OwnedBy(userId).FirstOrDefaultAsync(collection => collection.Id == collectionId, cancellationToken);

    public async Task<IReadOnlyCollection<Collection>> GetByPeakAsync(
        Guid peakId,
        CancellationToken cancellationToken) =>
        await context.Collections
            .Where(collection => collection.Peaks.Any(peak => peak.PeakId == peakId))
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(CollectionNameLookup lookup, CancellationToken cancellationToken)
    {
        IQueryable<Collection> named = context.Collections.Where(collection =>
            collection.ProfileId == lookup.ProfileId && collection.Name == lookup.Name);

        if (lookup.ExcludedCollectionId is { } excluded)
        {
            named = named.Where(collection => collection.Id != excluded);
        }

        return named.AnyAsync(cancellationToken);
    }

    public void Add(Collection collection) => context.Collections.Add(collection);

    public void Remove(Collection collection) => context.Collections.Remove(collection);

    private IQueryable<Collection> OwnedBy(Guid userId) =>
        context.Collections.Where(collection =>
            context.Profiles.Any(profile => profile.Id == collection.ProfileId && profile.UserId == userId));
}
