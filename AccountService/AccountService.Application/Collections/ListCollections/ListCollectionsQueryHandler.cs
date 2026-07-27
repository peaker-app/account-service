using AccountService.Application.Abstractions;
using Common.Application.Messaging;
using Common.Application.Pagination;
using Common.Domain.Results;

namespace AccountService.Application.Collections.ListCollections;

internal sealed class ListCollectionsQueryHandler(ICollectionReader collectionReader)
    : IQueryHandler<ListCollectionsQuery, PagedResult<CollectionSummaryResponse>>
{
    public async Task<Result<PagedResult<CollectionSummaryResponse>>> Handle(
        ListCollectionsQuery query,
        CancellationToken cancellationToken)
    {
        Result validation = query.Page.Validate();

        return validation.IsFailure
            ? Result.Failure<PagedResult<CollectionSummaryResponse>>(validation.Error)
            : await collectionReader.ListByUserAsync(query.UserId, query.Page, cancellationToken);
    }
}
