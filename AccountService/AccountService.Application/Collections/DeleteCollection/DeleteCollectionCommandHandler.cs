using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.DeleteCollection;

internal sealed class DeleteCollectionCommandHandler(
    ICollectionRepository collectionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCollectionCommand>
{
    public async Task<Result> Handle(DeleteCollectionCommand command, CancellationToken cancellationToken)
    {
        Collection? collection = await collectionRepository.GetOwnedByUserAsync(
            command.CollectionId, command.UserId, cancellationToken);

        if (collection is null)
        {
            return Result.Failure(CollectionErrors.NotFound(command.CollectionId));
        }

        Result deletable = collection.EnsureDeletable();
        if (deletable.IsFailure)
        {
            return deletable;
        }

        collectionRepository.Remove(collection);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
