using Common.Application.Pagination;

namespace AccountService.Application.Abstractions;

public sealed record CollectionDetailLookup(Guid CollectionId, Guid UserId, PageRequest Page);
