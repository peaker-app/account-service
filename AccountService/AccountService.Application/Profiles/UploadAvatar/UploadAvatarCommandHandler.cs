using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Images;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.UploadAvatar;

internal sealed class UploadAvatarCommandHandler(
    IProfileRepository profileRepository,
    IImageStorage imageStorage,
    IImageValidator imageValidator,
    IAvatarUrlSigner avatarUrlSigner,
    IUnitOfWork unitOfWork) : ICommandHandler<UploadAvatarCommand, AvatarResponse>
{
    public async Task<Result<AvatarResponse>> Handle(UploadAvatarCommand command, CancellationToken cancellationToken)
    {
        ImageRejection rejection = imageValidator.Validate(new ImageContent(
            command.Upload.Content,
            command.Upload.ContentType,
            AvatarConstraints.MaxSizeBytes));

        if (rejection is not ImageRejection.None)
        {
            return Result.Failure<AvatarResponse>(AvatarRejections.ToError(rejection));
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

        profile.SetAvatar(new Avatar(stored.Value.PublicId));
        await SaveOrDiscardAsync(stored.Value, cancellationToken);

        return new AvatarResponse(profile.SignAvatarUrl(avatarUrlSigner)!);
    }

    private async Task SaveOrDiscardAsync(StoredImage stored, CancellationToken cancellationToken)
    {
        bool persisted = false;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            persisted = true;
        }
        finally
        {
            if (!persisted)
            {
                await imageStorage.TryDeleteAsync(stored.PublicId, cancellationToken);
            }
        }
    }
}
