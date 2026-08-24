using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.ListCollections;
using Common.Application.Pagination;

namespace AccountService.API.Requests;

public sealed record ListCollectionsRequest(int Page = PageRequest.MinPage, int Size = PageRequest.DefaultSize)
{
    public ListCollectionsQuery ToQuery(Guid userId) => new(userId, new PageRequest(Page, Size));

    public GetCollectionByIdQuery ToQuery(Guid userId, Guid collectionId) =>
        new(userId, collectionId, new PageRequest(Page, Size));
}
