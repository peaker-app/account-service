using AccountService.Application.Abstractions;
using AccountService.Domain.Collections;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.GetCollectionById;

internal sealed class GetCollectionByIdQueryHandler(ICollectionReader collectionReader)
    : IQueryHandler<GetCollectionByIdQuery, CollectionDetailResponse>
{
    public async Task<Result<CollectionDetailResponse>> Handle(
        GetCollectionByIdQuery query,
        CancellationToken cancellationToken)
    {
        Result validation = query.Page.Validate();

        if (validation.IsFailure)
        {
            return Result.Failure<CollectionDetailResponse>(validation.Error);
        }

        var lookup = new CollectionDetailLookup(query.CollectionId, query.UserId, query.Page);
        CollectionDetailResponse? collection = await collectionReader.FindDetailAsync(lookup, cancellationToken);

        return collection is null
            ? Result.Failure<CollectionDetailResponse>(CollectionErrors.NotFound(query.CollectionId))
            : collection;
    }
}
