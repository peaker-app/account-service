using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Application.Pagination;
using Common.Domain.Results;

namespace AccountService.Application.Collections.ListCollections;

internal sealed class ListCollectionsQueryHandler(
    ICollectionReader collectionReader,
    IProfileRepository profileRepository)
    : IQueryHandler<ListCollectionsQuery, PagedResult<CollectionSummaryResponse>>
{
    public async Task<Result<PagedResult<CollectionSummaryResponse>>> Handle(
        ListCollectionsQuery query,
        CancellationToken cancellationToken)
    {
        Result validation = query.Page.Validate();

        if (validation.IsFailure)
        {
            return Result.Failure<PagedResult<CollectionSummaryResponse>>(validation.Error);
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return profile is null
            ? Result.Failure<PagedResult<CollectionSummaryResponse>>(ProfileErrors.NotFound(query.UserId))
            : await collectionReader.ListByUserAsync(query.UserId, query.Page, cancellationToken);
    }
}
