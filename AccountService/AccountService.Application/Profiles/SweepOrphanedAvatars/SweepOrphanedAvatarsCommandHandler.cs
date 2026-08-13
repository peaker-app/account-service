using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.SweepOrphanedAvatars;

internal sealed class SweepOrphanedAvatarsCommandHandler(
    IAvatarAssetInventory avatarAssetInventory,
    IImageStorage imageStorage,
    IProfileRepository profileRepository) : ICommandHandler<SweepOrphanedAvatarsCommand, AvatarSweepResponse>
{
    public async Task<Result<AvatarSweepResponse>> Handle(
        SweepOrphanedAvatarsCommand command,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StoredAsset> quarantined =
            await avatarAssetInventory.ListQuarantinedAsync(cancellationToken);

        IReadOnlyCollection<StoredAsset> confirmed =
            await avatarAssetInventory.ListConfirmedAsync(cancellationToken);

        return new AvatarSweepResponse(
            await DeleteOrphansAsync(quarantined, command.UploadedBeforeUtc, cancellationToken),
            await DeleteOrphansAsync(confirmed, command.UploadedBeforeUtc, cancellationToken));
    }

    private async Task<int> DeleteOrphansAsync(
        IReadOnlyCollection<StoredAsset> assets,
        DateTime uploadedBeforeUtc,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> candidates =
        [
            .. assets.Where(asset => asset.CreatedAtUtc < uploadedBeforeUtc).Select(asset => asset.PublicId)
        ];

        if (candidates.Count == 0)
        {
            return 0;
        }

        IReadOnlySet<string> referenced =
            await profileRepository.GetKnownAvatarPublicIdsAsync(candidates, cancellationToken);

        return await DeleteAllAsync(candidates.Except(referenced), cancellationToken);
    }

    private async Task<int> DeleteAllAsync(IEnumerable<string> publicIds, CancellationToken cancellationToken)
    {
        int removed = 0;

        foreach (string publicId in publicIds)
        {
            if (await imageStorage.TryDeleteAsync(publicId, cancellationToken))
            {
                removed++;
            }
        }

        return removed;
    }
}
