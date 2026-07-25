using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.ProfileAscents.SyncPeakName;

internal sealed class SyncPeakNameCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SyncPeakNameCommand>
{
    public async Task<Result> Handle(SyncPeakNameCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProfileAscent> affected =
            await ascentRepository.GetByPeakAsync(command.PeakId, cancellationToken);

        if (affected.Count == 0)
        {
            return Result.Success();
        }

        Result<PeakSnapshot> peak = PeakSnapshot.Create(
            command.PeakId, command.PeakName, command.PeakAltitudeMeters);

        if (peak.IsFailure)
        {
            return Result.Failure(peak.Error);
        }

        foreach (ProfileAscent record in affected)
        {
            record.SyncPeak(peak.Value);
        }

        await RefreshProfilesAsync(affected, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task RefreshProfilesAsync(
        IReadOnlyCollection<ProfileAscent> affected,
        CancellationToken cancellationToken)
    {
        foreach (Guid profileId in affected.Select(record => record.ProfileId).Distinct())
        {
            await RefreshProfileAsync(profileId, cancellationToken);
        }
    }

    private async Task RefreshProfileAsync(Guid profileId, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByIdAsync(profileId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profileId, cancellationToken);

        profile.RefreshStats(ProfileStatsCalculator.Calculate(ascents), dateTimeProvider.UtcNow);
    }
}
