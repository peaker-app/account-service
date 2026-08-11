using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.ProfileAscents.RecordAscent;

internal sealed class RecordAscentCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<RecordAscentCommandHandler> logger) : ICommandHandler<RecordAscentCommand>
{
    public async Task<Result> Handle(RecordAscentCommand command, CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            logger.LogWarning(
                "Discarded ascent {AscentId}: no profile exists for user {UserId}",
                command.AscentId,
                command.UserId);

            return Result.Success();
        }

        Result<ProfileAscentValues> values = Read(command);

        return values.IsFailure
            ? Result.Failure(values.Error)
            : await ProjectAsync(profile, command, values.Value, cancellationToken);
    }

    private static Result<ProfileAscentValues> Read(RecordAscentCommand command)
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
            : new ProfileAscentValues(peak.Value, command.AscentDate, visibility.Value);
    }

    private async Task<Result> ProjectAsync(
        Profile profile,
        RecordAscentCommand command,
        ProfileAscentValues values,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profile.Id, cancellationToken);

        ProfileAscent? existing = ascents.FirstOrDefault(ascent => ascent.AscentId == command.AscentId);

        if (existing is not null)
        {
            existing.SyncPeak(values.Peak);
            existing.Sync(values.AscentDate, values.Visibility);

            return await RefreshAsync(profile, ascents, cancellationToken);
        }

        Result<ProfileAscent> record = ProfileAscent.Create(new ProfileAscentDraft(
            profile.Id, command.AscentId, values.Peak, values.AscentDate, values.Visibility));

        if (record.IsFailure)
        {
            return Result.Failure(record.Error);
        }

        ascentRepository.Add(record.Value);

        return await RefreshAsync(profile, [.. ascents, record.Value], cancellationToken);
    }

    private async Task<Result> RefreshAsync(
        Profile profile,
        IReadOnlyCollection<ProfileAscent> ascents,
        CancellationToken cancellationToken)
    {
        profile.RefreshStats(ProfileStatsCalculator.Calculate(ascents), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private sealed record ProfileAscentValues(PeakSnapshot Peak, DateOnly AscentDate, AscentVisibility Visibility);
}
