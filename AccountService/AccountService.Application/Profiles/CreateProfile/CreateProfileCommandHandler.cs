using AccountService.Domain.Collections;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.CreateProfile;

internal sealed class CreateProfileCommandHandler(
    IProfileRepository profileRepository,
    ICollectionRepository collectionRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateProfileCommand>
{
    public async Task<Result> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        if (await profileRepository.ExistsByUserIdAsync(command.UserId, cancellationToken))
        {
            return Result.Success();
        }

        Result<DisplayName> displayName = DisplayName.Create(command.Username);
        if (displayName.IsFailure)
        {
            return Result.Failure(displayName.Error);
        }

        ProfileSlug slug = await ResolveUniqueSlugAsync(command.Username, cancellationToken);
        Result<Profile> profile = Profile.Create(
            new ProfileDraft(command.UserId, displayName.Value, slug), dateTimeProvider.UtcNow);

        if (profile.IsFailure)
        {
            return Result.Failure(profile.Error);
        }

        Result<Collection> defaultCollection = Collection.CreateDefault(profile.Value.Id);
        if (defaultCollection.IsFailure)
        {
            return Result.Failure(defaultCollection.Error);
        }

        profileRepository.Add(profile.Value);
        collectionRepository.Add(defaultCollection.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<ProfileSlug> ResolveUniqueSlugAsync(string username, CancellationToken cancellationToken)
    {
        ProfileSlug baseSlug = ProfileSlug.FromUsername(username);
        ProfileSlug candidate = baseSlug;

        for (int suffix = 2; await profileRepository.ExistsBySlugAsync(candidate.Value, cancellationToken); suffix++)
        {
            candidate = baseSlug.WithSuffix(suffix);
        }

        return candidate;
    }
}
