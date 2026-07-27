using Common.Application.Pagination;

namespace AccountService.Application.Collections.GetCollectionById;

public sealed record CollectionDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Kind,
    int PeakCount,
    PagedResult<CollectionPeakResponse> Peaks);
