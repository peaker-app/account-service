using Common.Domain.Results;

namespace AccountService.Application.Abstractions;

public sealed record AvatarUpload(ReadOnlyMemory<byte> Content, string ContentType, string FileName);

public sealed record StoredImage(string PublicId);

public interface IImageStorage
{
    Task<Result<StoredImage>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken);

    Task ConfirmAsync(string publicId, CancellationToken cancellationToken);

    Task DeleteAsync(string publicId, CancellationToken cancellationToken);

    Task<bool> TryDeleteAsync(string publicId, CancellationToken cancellationToken);
}
