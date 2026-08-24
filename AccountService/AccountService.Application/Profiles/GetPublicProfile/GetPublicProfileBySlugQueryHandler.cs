using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.GetPublicProfile;

internal sealed class GetPublicProfileBySlugQueryHandler(
    IProfileRepository profileRepository,
    IAvatarUrlSigner avatarUrlSigner)
    : IQueryHandler<GetPublicProfileBySlugQuery, PublicProfileResponse>
{
    public async Task<Result<PublicProfileResponse>> Handle(
        GetPublicProfileBySlugQuery query,
        CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetBySlugAsync(query.Slug, cancellationToken);

        return profile is not null && profile.IsVisibleTo(query.RequesterId)
            ? profile.ToPublicResponse(avatarUrlSigner)
            : Result.Failure<PublicProfileResponse>(ProfileErrors.NotFoundBySlug(query.Slug));
    }
}
