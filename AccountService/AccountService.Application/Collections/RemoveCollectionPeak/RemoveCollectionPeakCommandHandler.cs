using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.RemoveCollectionPeak;

internal sealed class RemoveCollectionPeakCommandHandler(
    ICollectionRepository collectionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveCollectionPeakCommand>
{
    public async Task<Result> Handle(RemoveCollectionPeakCommand command, CancellationToken cancellationToken)
    {
        Collection? collection = await collectionRepository.GetOwnedByUserAsync(
            command.CollectionId, command.UserId, cancellationToken);

        if (collection is null)
        {
            return Result.Failure(CollectionErrors.NotFound(command.CollectionId));
        }

        Result removed = collection.RemovePeak(command.PeakId);
        if (removed.IsFailure)
        {
            return removed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
