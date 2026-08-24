using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.GetMyStats;

internal sealed class GetMyStatsQueryHandler(IProfileRepository profileRepository)
    : IQueryHandler<GetMyStatsQuery, ProfileStatsResponse>
{
    public async Task<Result<ProfileStatsResponse>> Handle(
        GetMyStatsQuery query,
        CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return profile is null
            ? Result.Failure<ProfileStatsResponse>(ProfileErrors.NotFound(query.UserId))
            : profile.Stats.Overall.ToResponse();
    }
}
