using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.UpdateCollection;

internal sealed class UpdateCollectionCommandHandler(
    ICollectionRepository collectionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCollectionCommand>
{
    public async Task<Result> Handle(UpdateCollectionCommand command, CancellationToken cancellationToken)
    {
        Result<CollectionName> name = CollectionName.Create(command.Name);
        if (name.IsFailure)
        {
            return Result.Failure(name.Error);
        }

        Collection? collection = await collectionRepository.GetOwnedByUserAsync(
            command.CollectionId, command.UserId, cancellationToken);

        return collection is null
            ? Result.Failure(CollectionErrors.NotFound(command.CollectionId))
            : await ApplyAsync(collection, new CollectionDetails(name.Value, command.Description), cancellationToken);
    }

    private async Task<Result> ApplyAsync(
        Collection collection,
        CollectionDetails details,
        CancellationToken cancellationToken)
    {
        Result editable = collection.EnsureEditable();
        if (editable.IsFailure)
        {
            return editable;
        }

        var lookup = new CollectionNameLookup(collection.ProfileId, details.Name, collection.Id);

        if (await collectionRepository.ExistsByNameAsync(lookup, cancellationToken))
        {
            return Result.Failure(CollectionErrors.NameAlreadyUsed);
        }

        Result updated = collection.UpdateDetails(details);
        if (updated.IsFailure)
        {
            return updated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
