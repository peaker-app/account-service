using Common.Domain.Results;

namespace AccountService.Application.Abstractions;

public sealed record AvatarUpload(ReadOnlyMemory<byte> Content, string ContentType, string FileName);

public sealed record StoredImage(string PublicId, string SecureUrl);

public interface IImageStorage
{
    Task<Result<StoredImage>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken);

    Task DeleteAsync(string publicId, CancellationToken cancellationToken);
}
