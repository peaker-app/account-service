using AccountService.Domain.Collections;
using Common.Domain.Results;

namespace AccountService.Application.Abstractions;

public interface IPeakCatalog
{
    Task<Result<CollectionPeakSnapshot>> GetSnapshotAsync(Guid peakId, CancellationToken cancellationToken);
}
