using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.ChangeSlug;

internal sealed class ChangeSlugCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeSlugCommand>
{
    public async Task<Result> Handle(ChangeSlugCommand command, CancellationToken cancellationToken)
    {
        Result<ProfileSlug> slug = ProfileSlug.Create(command.Slug);
        if (slug.IsFailure)
        {
            return Result.Failure(slug.Error);
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        if (profile.Slug == slug.Value)
        {
            return Result.Success();
        }

        if (await profileRepository.ExistsBySlugAsync(slug.Value.Value, cancellationToken))
        {
            return Result.Failure(ProfileErrors.SlugAlreadyTaken);
        }

        profile.ChangeSlug(slug.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
