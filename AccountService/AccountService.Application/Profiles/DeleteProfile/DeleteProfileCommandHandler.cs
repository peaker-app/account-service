using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.DeleteProfile;

internal sealed class DeleteProfileCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProfileCommand>
{
    public async Task<Result> Handle(DeleteProfileCommand command, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Success();
        }

        profileRepository.Remove(profile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
