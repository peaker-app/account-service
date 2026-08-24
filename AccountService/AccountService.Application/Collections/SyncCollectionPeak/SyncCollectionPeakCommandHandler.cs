using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.SyncCollectionPeak;

internal sealed class SyncCollectionPeakCommandHandler(
    ICollectionRepository collectionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SyncCollectionPeakCommand>
{
    public async Task<Result> Handle(SyncCollectionPeakCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Collection> affected =
            await collectionRepository.GetByPeakAsync(command.PeakId, cancellationToken);

        if (affected.Count == 0)
        {
            return Result.Success();
        }

        Result<CollectionPeakSnapshot> peak = CollectionPeakSnapshot.Create(
            command.PeakId, command.PeakName, command.PeakAltitudeMeters);

        if (peak.IsFailure)
        {
            return Result.Failure(peak.Error);
        }

        bool changed = false;

        foreach (Collection collection in affected)
        {
            changed |= collection.SyncPeak(peak.Value);
        }

        return changed ? await CommitAsync(cancellationToken) : Result.Success();
    }

    private async Task<Result> CommitAsync(CancellationToken cancellationToken)
    {
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
