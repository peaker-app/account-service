using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common.Domain.Results;
using Common.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.ExternalServices;

internal sealed class CloudinaryImageStorage(
    CloudinaryFactory cloudinaryFactory,
    CompensationMetrics compensationMetrics,
    ILogger<CloudinaryImageStorage> logger) : IImageStorage
{
    private const string DeletedOutcome = "ok";
    private const string MissingOutcome = "not found";
    private const string AssetKind = "profile-avatar";

    public async Task<Result<StoredImage>> UploadAvatarAsync(
        AvatarUpload upload,
        CancellationToken cancellationToken)
    {
        using MemoryStream stream = new(upload.Content.ToArray());

        ImageUploadParams uploadParameters = new()
        {
            File = new FileDescription(upload.FileName, stream),
            Folder = cloudinaryFactory.Options.Folder,
            Type = AvatarDelivery.AuthenticatedType,
            Tags = AvatarDelivery.QuarantineTag,
            Format = AvatarDelivery.StoredFormat,
            Transformation = AvatarDelivery.Sanitizing(),
            Overwrite = false
        };

        ImageUploadResult result = await cloudinaryFactory.Client.UploadAsync(uploadParameters, cancellationToken);

        if (result.Error is not null)
        {
            logger.LogWarning("Cloudinary avatar upload failed: {ErrorMessage}", result.Error.Message);
            return Result.Failure<StoredImage>(ProfileErrors.AvatarUploadFailed);
        }

        return new StoredImage(result.PublicId);
    }

    public async Task ConfirmAsync(string publicId, CancellationToken cancellationToken)
    {
        TagParams tagParameters = new()
        {
            Command = TagCommand.Remove,
            Tag = AvatarDelivery.QuarantineTag,
            Type = AvatarDelivery.AuthenticatedType,
            PublicIds = [publicId]
        };

        TagResult result = await cloudinaryFactory.Client.TagAsync(tagParameters, cancellationToken);

        if (result.Error is null)
        {
            return;
        }

        logger.LogWarning(
            "Cloudinary avatar {PublicId} could not leave quarantine: {ErrorMessage}",
            publicId,
            result.Error.Message);

        throw new ImageStorageException($"Cloudinary did not confirm the storage of '{publicId}'.");
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DeletionParams deletionParameters = new(publicId)
        {
            Type = AvatarDelivery.AuthenticatedType,
            Invalidate = true
        };

        DeletionResult result = await cloudinaryFactory.Client.DestroyAsync(deletionParameters);

        if (IsConfirmed(result))
        {
            return;
        }

        logger.LogWarning(
            "Cloudinary avatar deletion was not confirmed for {PublicId}: {Outcome}",
            publicId,
            result.Error?.Message ?? result.Result);

        throw new ImageStorageException($"Cloudinary did not confirm the deletion of '{publicId}'.");
    }

    public async Task<bool> TryDeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        try
        {
            await DeleteAsync(publicId, cancellationToken);

            return true;
        }
        catch (ImageStorageException exception)
        {
            logger.LogError(exception, "Orphaned Cloudinary avatar {PublicId} could not be removed", publicId);
            compensationMetrics.RecordFailure(AssetKind);

            return false;
        }
    }

    private static bool IsConfirmed(DeletionResult result) =>
        result.Error is null &&
        (string.Equals(result.Result, DeletedOutcome, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(result.Result, MissingOutcome, StringComparison.OrdinalIgnoreCase));
}
