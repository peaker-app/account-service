using System.Collections.Concurrent;
using AccountService.Application.Abstractions;
using AccountService.Domain.Collections;
using Common.Domain.Results;

namespace AccountService.IntegrationTests.Fakes;

internal sealed class FakePeakCatalog : IPeakCatalog
{
    private readonly ConcurrentDictionary<Guid, CollectionPeakSnapshot> _peaks = new();

    public bool IsAvailable { get; set; } = true;

    public Guid Register(string name = "Aneto", int altitudeMeters = 3404)
    {
        Guid peakId = Guid.CreateVersion7();
        _peaks[peakId] = CollectionPeakSnapshot.Create(peakId, name, altitudeMeters).Value;

        return peakId;
    }

    public Task<Result<CollectionPeakSnapshot>> GetSnapshotAsync(
        Guid peakId,
        CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            return Task.FromResult(
                Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakCatalogUnavailable));
        }

        return Task.FromResult(_peaks.TryGetValue(peakId, out CollectionPeakSnapshot? peak)
            ? Result.Success(peak)
            : Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakNotFound(peakId)));
    }
}
