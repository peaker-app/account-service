using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.ProfileAscents.RecordAscent;

internal sealed class RecordAscentCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RecordAscentCommand>
{
    public async Task<Result> Handle(RecordAscentCommand command, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        if (await ascentRepository.ExistsByAscentIdAsync(command.AscentId, cancellationToken))
        {
            return Result.Success();
        }

        Result<ProfileAscent> record = BuildRecord(command, profile.Id);

        if (record.IsFailure)
        {
            return Result.Failure(record.Error);
        }

        await AppendAndRefreshAsync(profile, record.Value, cancellationToken);

        return Result.Success();
    }

    private static Result<ProfileAscent> BuildRecord(RecordAscentCommand command, Guid profileId)
    {
        Result<PeakSnapshot> peak = PeakSnapshot.Create(
            command.PeakId, command.PeakName, command.PeakAltitudeMeters);

        if (peak.IsFailure)
        {
            return peak.Error;
        }

        Result<AscentVisibility> visibility = AscentVisibilities.Parse(command.Visibility);

        return visibility.IsFailure
            ? visibility.Error
            : ProfileAscent.Create(
                new ProfileAscentDraft(profileId, command.AscentId, peak.Value, command.AscentDate, visibility.Value));
    }

    private async Task AppendAndRefreshAsync(
        Profile profile,
        ProfileAscent record,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profile.Id, cancellationToken);

        ascentRepository.Add(record);
        profile.RefreshStats(ProfileStatsCalculator.Calculate([.. ascents, record]), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
