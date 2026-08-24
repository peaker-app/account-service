using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.ProfileAscents.RemoveAscent;

internal sealed class RemoveAscentCommandHandler(
    IProfileRepository profileRepository,
    IProfileAscentRepository ascentRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveAscentCommand>
{
    public async Task<Result> Handle(RemoveAscentCommand command, CancellationToken cancellationToken)
    {
        ProfileAscent? record = await ascentRepository.GetByAscentIdAsync(command.AscentId, cancellationToken);

        if (record is null)
        {
            return Result.Success();
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        await RemoveAndRefreshAsync(profile, record, cancellationToken);

        return Result.Success();
    }

    private async Task RemoveAndRefreshAsync(
        Profile profile,
        ProfileAscent record,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProfileAscent> ascents =
            await ascentRepository.GetByProfileAsync(profile.Id, cancellationToken);

        ascentRepository.Remove(record);

        ProfileAscent[] remaining = [.. ascents.Where(ascent => ascent.Id != record.Id)];
        profile.RefreshStats(ProfileStatsCalculator.Calculate(remaining), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
