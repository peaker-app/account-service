using AccountService.Application.Abstractions;
using CloudinaryDotNet;
using Common.Application.Abstractions;

namespace AccountService.Infrastructure.ExternalServices;

internal sealed class CloudinaryAvatarUrlSigner(
    CloudinaryFactory cloudinaryFactory,
    IDateTimeProvider dateTimeProvider) : IAvatarUrlSigner
{
    public string Sign(string publicId, TimeSpan lifetime)
    {
        AuthToken token = new AuthToken(cloudinaryFactory.Options.AuthTokenKey)
            .StartTime(new DateTimeOffset(dateTimeProvider.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds())
            .Duration((long)lifetime.TotalSeconds);

        return cloudinaryFactory.Client.Api.UrlImgUp
            .Secure(true)
            .Type(AvatarDelivery.AuthenticatedType)
            .Signed(true)
            .AuthToken(token)
            .BuildUrl(publicId);
    }
}
