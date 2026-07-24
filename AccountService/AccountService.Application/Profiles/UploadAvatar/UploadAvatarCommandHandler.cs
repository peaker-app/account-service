using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.UploadAvatar;

internal sealed class UploadAvatarCommandHandler(
    IProfileRepository profileRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork) : ICommandHandler<UploadAvatarCommand, AvatarResponse>
{
    public async Task<Result<AvatarResponse>> Handle(UploadAvatarCommand command, CancellationToken cancellationToken)
    {
        if (command.Upload.Content.Length > AvatarConstraints.MaxSizeBytes)
        {
            return Result.Failure<AvatarResponse>(ProfileErrors.AvatarTooLarge);
        }

        if (!ImageFormatInspector.IsSupported(command.Upload.Content.Span))
        {
            return Result.Failure<AvatarResponse>(ProfileErrors.AvatarFormatNotSupported);
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<AvatarResponse>(ProfileErrors.NotFound(command.UserId));
        }

        Result<StoredImage> stored = await imageStorage.UploadAvatarAsync(command.Upload, cancellationToken);
        if (stored.IsFailure)
        {
            return Result.Failure<AvatarResponse>(stored.Error);
        }

        profile.SetAvatar(new Avatar(stored.Value.PublicId, stored.Value.SecureUrl));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AvatarResponse(stored.Value.SecureUrl);
    }
}
