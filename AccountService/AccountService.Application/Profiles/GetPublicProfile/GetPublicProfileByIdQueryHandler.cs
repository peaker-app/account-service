using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.GetPublicProfile;

internal sealed class GetPublicProfileByIdQueryHandler(IProfileRepository profileRepository)
    : IQueryHandler<GetPublicProfileByIdQuery, PublicProfileResponse>
{
    public async Task<Result<PublicProfileResponse>> Handle(
        GetPublicProfileByIdQuery query,
        CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(query.TargetUserId, cancellationToken);

        return profile is not null && profile.IsVisibleTo(query.RequesterId)
            ? profile.ToPublicResponse()
            : Result.Failure<PublicProfileResponse>(ProfileErrors.NotFound(query.TargetUserId));
    }
}
