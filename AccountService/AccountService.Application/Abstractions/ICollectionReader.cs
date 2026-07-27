using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.ListCollections;
using Common.Application.Pagination;

namespace AccountService.Application.Abstractions;

public interface ICollectionReader
{
    Task<PagedResult<CollectionSummaryResponse>> ListByUserAsync(
        Guid userId,
        PageRequest page,
        CancellationToken cancellationToken);

    Task<CollectionDetailResponse?> FindDetailAsync(
        CollectionDetailLookup lookup,
        CancellationToken cancellationToken);
}
