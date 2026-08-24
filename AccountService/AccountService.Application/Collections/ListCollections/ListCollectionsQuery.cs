using Common.Application.Messaging;
using Common.Application.Pagination;

namespace AccountService.Application.Collections.ListCollections;

public sealed record ListCollectionsQuery(Guid UserId, PageRequest Page)
    : IQuery<PagedResult<CollectionSummaryResponse>>;
