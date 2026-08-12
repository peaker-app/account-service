using AccountService.Domain.Collections;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Collections.CreateCollection;

internal sealed class CreateCollectionCommandHandler(
    ICollectionRepository collectionRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCollectionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCollectionCommand command, CancellationToken cancellationToken)
    {
        Result<CollectionName> name = CollectionName.Create(command.Name);
        if (name.IsFailure)
        {
            return Result.Failure<Guid>(name.Error);
        }

        Guid? profileId = await profileRepository.FindIdByUserIdAsync(command.UserId, cancellationToken);
        if (profileId is null)
        {
            return Result.Failure<Guid>(ProfileErrors.NotFound(command.UserId));
        }

        CollectionDetails details = new(name.Value, command.Description);
        CollectionNameLookup lookup = new(profileId.Value, name.Value);

        if (await collectionRepository.ExistsByNameAsync(lookup, cancellationToken))
        {
            return Result.Failure<Guid>(CollectionErrors.NameAlreadyUsed);
        }

        return await collectionRepository.CountByProfileAsync(profileId.Value, cancellationToken)
            >= Collection.MaxPerProfile
            ? Result.Failure<Guid>(CollectionErrors.CollectionLimitReached)
            : await CreateAsync(profileId.Value, details, cancellationToken);
    }

    private async Task<Result<Guid>> CreateAsync(
        Guid profileId,
        CollectionDetails details,
        CancellationToken cancellationToken)
    {
        Result<Collection> collection = Collection.Create(new CollectionDraft(profileId, details));

        if (collection.IsFailure)
        {
            return Result.Failure<Guid>(collection.Error);
        }

        collectionRepository.Add(collection.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return collection.Value.Id;
    }
}
