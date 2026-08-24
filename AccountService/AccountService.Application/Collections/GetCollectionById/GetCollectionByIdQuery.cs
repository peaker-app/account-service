using Common.Application.Messaging;
using Common.Application.Pagination;

namespace AccountService.Application.Collections.GetCollectionById;

public sealed record GetCollectionByIdQuery(Guid UserId, Guid CollectionId, PageRequest Page)
    : IQuery<CollectionDetailResponse>;
