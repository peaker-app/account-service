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
        Url url = cloudinaryFactory.Client.Api.UrlImgUp
            .Secure(true)
            .Type(AvatarDelivery.AuthenticatedType)
            .Signed(true);

        return HasTokenKey
            ? url.AuthToken(ExpiringToken(lifetime)).BuildUrl(publicId)
            : url.BuildUrl(publicId);
    }

    private bool HasTokenKey => !string.IsNullOrWhiteSpace(cloudinaryFactory.Options.AuthTokenKey);

    private AuthToken ExpiringToken(TimeSpan lifetime) =>
        new AuthToken(cloudinaryFactory.Options.AuthTokenKey)
            .StartTime(new DateTimeOffset(dateTimeProvider.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds())
            .Duration((long)lifetime.TotalSeconds);
}
