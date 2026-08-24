using System.Globalization;
using AccountService.Application.Abstractions;
using AccountService.Domain.Collections;
using Common.Domain.Results;
using Microsoft.Extensions.Caching.Memory;

namespace AccountService.Infrastructure.ExternalServices;

public sealed class CachingPeakCatalog(IPeakCatalog inner, IMemoryCache cache) : IPeakCatalog
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    public async Task<Result<CollectionPeakSnapshot>> GetSnapshotAsync(
        Guid peakId,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(KeyFor(peakId), out CollectionPeakSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        Result<CollectionPeakSnapshot> snapshot = await inner.GetSnapshotAsync(peakId, cancellationToken);

        if (snapshot.IsSuccess)
        {
            cache.Set(KeyFor(peakId), snapshot.Value, Lifetime);
        }

        return snapshot;
    }

    public static void Evict(IMemoryCache cache, Guid peakId) => cache.Remove(KeyFor(peakId));

    private static string KeyFor(Guid peakId) =>
        string.Create(CultureInfo.InvariantCulture, $"peak-catalog:{peakId}");
}
