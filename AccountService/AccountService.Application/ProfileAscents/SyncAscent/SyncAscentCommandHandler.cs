using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.ProfileAscents.SyncAscent;

internal sealed class SyncAscentCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SyncAscentCommand>
{
    public async Task<Result> Handle(SyncAscentCommand command, CancellationToken cancellationToken)
    {
        ProfileAscent? record = await ascentRepository.GetByAscentIdAsync(command.AscentId, cancellationToken);

        if (record is null)
        {
            return Result.Success();
        }

        Result<AscentVisibility> visibility = AscentVisibilities.Parse(command.Visibility);

        if (visibility.IsFailure)
        {
            return Result.Failure(visibility.Error);
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        record.Sync(command.AscentDate, visibility.Value);

        await RefreshAsync(profile, cancellationToken);

        return Result.Success();
    }

    private async Task RefreshAsync(Profile profile, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profile.Id, cancellationToken);

        profile.RefreshStats(ProfileStatsCalculator.Calculate(ascents), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
