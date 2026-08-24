using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.RemoveAvatar;

internal sealed class RemoveAvatarCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveAvatarCommand>
{
    public async Task<Result> Handle(RemoveAvatarCommand command, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        profile.RemoveAvatar();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
