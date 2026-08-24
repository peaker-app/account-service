using AccountService.Domain.Profiles;
using Common.Application.Images;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.UploadAvatar;

internal static class AvatarRejections
{
    public static Error ToError(ImageRejection rejection) => rejection switch
    {
        ImageRejection.TooLarge => ProfileErrors.AvatarTooLarge,
        _ => ProfileErrors.AvatarFormatNotSupported
    };
}
