using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.ProfileAscents.RebuildProfileStats;

internal sealed class RebuildProfileStatsCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RebuildProfileStatsCommand, ProfileStatsRebuildResponse>
{
    public async Task<Result<ProfileStatsRebuildResponse>> Handle(
        RebuildProfileStatsCommand command,
        CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<ProfileStatsRebuildResponse>(ProfileErrors.NotFound(command.UserId));
        }

        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profile.Id, cancellationToken);

        profile.RefreshStats(ProfileStatsCalculator.Calculate(ascents), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProfileStatsRebuildResponse(command.UserId, ascents.Count);
    }
}
