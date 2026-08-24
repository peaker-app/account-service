using AccountService.Application.Abstractions;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.Mappings;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.AddCollectionPeak;

internal sealed class AddCollectionPeakCommandHandler(
    ICollectionRepository collectionRepository,
    IPeakCatalog peakCatalog,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<AddCollectionPeakCommand, CollectionPeakResponse>
{
    public async Task<Result<CollectionPeakResponse>> Handle(
        AddCollectionPeakCommand command,
        CancellationToken cancellationToken)
    {
        Collection? collection = await collectionRepository.GetOwnedByUserAsync(
            command.CollectionId, command.UserId, cancellationToken);

        if (collection is null)
        {
            return Result.Failure<CollectionPeakResponse>(CollectionErrors.NotFound(command.CollectionId));
        }

        Result<CollectionPeakSnapshot> peak =
            await peakCatalog.GetSnapshotAsync(command.PeakId, cancellationToken);

        return peak.IsFailure
            ? Result.Failure<CollectionPeakResponse>(peak.Error)
            : await AddAsync(collection, peak.Value, cancellationToken);
    }

    private async Task<Result<CollectionPeakResponse>> AddAsync(
        Collection collection,
        CollectionPeakSnapshot peak,
        CancellationToken cancellationToken)
    {
        Result<CollectionPeak> added = collection.AddPeak(peak, dateTimeProvider.UtcNow);

        if (added.IsFailure)
        {
            return Result.Failure<CollectionPeakResponse>(added.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return added.Value.ToResponse();
    }
}
