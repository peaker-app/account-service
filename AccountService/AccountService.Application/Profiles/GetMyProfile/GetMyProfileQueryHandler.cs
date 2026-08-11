using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.GetMyProfile;

internal sealed class GetMyProfileQueryHandler(IProfileRepository profileRepository, IAvatarUrlSigner avatarUrlSigner)
    : IQueryHandler<GetMyProfileQuery, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return profile is null
            ? Result.Failure<ProfileResponse>(ProfileErrors.NotFound(query.UserId))
            : profile.ToResponse(avatarUrlSigner);
    }
}
