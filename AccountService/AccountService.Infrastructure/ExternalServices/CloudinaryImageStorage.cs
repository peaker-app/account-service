using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccountService.Infrastructure.ExternalServices;

internal sealed class CloudinaryImageStorage : IImageStorage
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;
    private readonly ILogger<CloudinaryImageStorage> _logger;

    public CloudinaryImageStorage(IOptions<CloudinaryOptions> options, ILogger<CloudinaryImageStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _cloudinary = new Cloudinary(new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret));
        _cloudinary.Api.Secure = true;
    }

    public async Task<Result<StoredImage>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(upload.Content.ToArray());

        var uploadParameters = new ImageUploadParams
        {
            File = new FileDescription(upload.FileName, stream),
            Folder = _options.Folder,
            Overwrite = true
        };

        ImageUploadResult result = await _cloudinary.UploadAsync(uploadParameters, cancellationToken);

        if (result.Error is not null)
        {
            _logger.LogWarning("Cloudinary avatar upload failed: {ErrorMessage}", result.Error.Message);
            return Result.Failure<StoredImage>(ProfileErrors.AvatarUploadFailed);
        }

        return new StoredImage(result.PublicId, result.SecureUrl.ToString());
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var deletionParameters = new DeletionParams(publicId);

        await _cloudinary.DestroyAsync(deletionParameters);
    }

    public async Task TryDeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        try
        {
            await DeleteAsync(publicId, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            // Motivo: compensar una subida que no llegó a persistirse es best-effort. Si Cloudinary
            // no responde no puede convertirse el error del caso de uso en un 500.
            _logger.LogError(exception, "Orphaned Cloudinary avatar {PublicId} could not be removed", publicId);
        }
    }
}
