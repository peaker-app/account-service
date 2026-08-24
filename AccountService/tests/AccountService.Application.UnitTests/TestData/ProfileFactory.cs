using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using NSubstitute;

namespace AccountService.Application.UnitTests.TestData;

internal static class ProfileFactory
{
    public const string SignedUrlPrefix = "https://res.cloudinary.test/image/authenticated/";

    public static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static string SignedUrlFor(string publicId, TimeSpan lifetime) =>
        $"{SignedUrlPrefix}{publicId}?expires={lifetime.TotalSeconds}";

    public static IAvatarUrlSigner AvatarUrlSigner()
    {
        IAvatarUrlSigner signer = Substitute.For<IAvatarUrlSigner>();

        signer.Sign(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(call => SignedUrlFor(call.ArgAt<string>(0), call.ArgAt<TimeSpan>(1)));

        return signer;
    }

    public static Profile For(Guid userId, string username = "hiker")
    {
        DisplayName displayName = DisplayName.Create(username).Value;
        ProfileSlug slug = ProfileSlug.FromUsername(username);

        return Profile.Create(new ProfileDraft(userId, displayName, slug), Now).Value;
    }
}
