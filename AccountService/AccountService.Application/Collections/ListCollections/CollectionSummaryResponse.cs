namespace AccountService.Application.Collections.ListCollections;

public sealed record CollectionSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string Kind,
    int PeakCount);
