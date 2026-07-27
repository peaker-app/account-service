using AccountService.Application.Abstractions;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.ListCollections;
using AccountService.Domain.Collections;
using Common.Application.Pagination;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

internal sealed class CollectionReader(AccountDbContext context) : ICollectionReader
{
    public async Task<PagedResult<CollectionSummaryResponse>> ListByUserAsync(
        Guid userId,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        IQueryable<Collection> source = OwnedBy(userId);
        int totalCount = await source.CountAsync(cancellationToken);

        List<CollectionSummaryResponse> items = await source
            .AsNoTracking()
            .OrderBy(collection => collection.Kind == CollectionKind.WantToClimb ? 0 : 1)
            .ThenBy(collection => collection.Name)
            .ThenBy(collection => collection.Id)
            .Skip(page.Skip)
            .Take(page.Size)
            .Select(collection => new CollectionSummaryResponse(
                collection.Id,
                collection.Name.Value,
                collection.Description,
                collection.Kind.ToString(),
                collection.Peaks.Count()))
            .ToListAsync(cancellationToken);

        return new PagedResult<CollectionSummaryResponse>(items, page.Page, page.Size, totalCount);
    }

    public async Task<CollectionDetailResponse?> FindDetailAsync(
        CollectionDetailLookup lookup,
        CancellationToken cancellationToken)
    {
        CollectionSummaryResponse? header = await OwnedBy(lookup.UserId)
            .AsNoTracking()
            .Where(collection => collection.Id == lookup.CollectionId)
            .Select(collection => new CollectionSummaryResponse(
                collection.Id,
                collection.Name.Value,
                collection.Description,
                collection.Kind.ToString(),
                collection.Peaks.Count()))
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return null;
        }

        PagedResult<CollectionPeakResponse> peaks = await PaginatePeaksAsync(lookup, header.PeakCount, cancellationToken);

        return new CollectionDetailResponse(
            header.Id,
            header.Name,
            header.Description,
            header.Kind,
            header.PeakCount,
            peaks);
    }

    private async Task<PagedResult<CollectionPeakResponse>> PaginatePeaksAsync(
        CollectionDetailLookup lookup,
        int totalCount,
        CancellationToken cancellationToken)
    {
        List<CollectionPeakResponse> items = await OwnedBy(lookup.UserId)
            .AsNoTracking()
            .Where(collection => collection.Id == lookup.CollectionId)
            .SelectMany(collection => collection.Peaks)
            .OrderBy(peak => peak.AddedAtUtc)
            .ThenBy(peak => peak.Id)
            .Skip(lookup.Page.Skip)
            .Take(lookup.Page.Size)
            .Select(peak => new CollectionPeakResponse(
                peak.Id,
                peak.PeakId,
                peak.PeakName,
                peak.PeakAltitudeMeters,
                peak.AddedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<CollectionPeakResponse>(items, lookup.Page.Page, lookup.Page.Size, totalCount);
    }

    private IQueryable<Collection> OwnedBy(Guid userId) =>
        context.Collections.Where(collection =>
            context.Profiles.Any(profile => profile.Id == collection.ProfileId && profile.UserId == userId));
}
