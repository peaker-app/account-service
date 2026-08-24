using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.Mappings;
using AccountService.Domain.Profiles;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.ExportMyData;

internal sealed class ExportMyProfileQueryHandler(
    IProfileRepository profileRepository,
    ICollectionReader collectionReader,
    IAvatarUrlSigner avatarUrlSigner) : IQueryHandler<ExportMyProfileQuery, ProfileExportResponse>
{
    public async Task<Result<ProfileExportResponse>> Handle(
        ExportMyProfileQuery query,
        CancellationToken cancellationToken)
    {
        Profile? profile = await profileRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<ProfileExportResponse>(ProfileErrors.NotFound(query.UserId));
        }

        IReadOnlyList<ExportedCollectionResponse> collections =
            await collectionReader.ExportByUserAsync(query.UserId, cancellationToken);

        return new ProfileExportResponse(
            profile.ToResponse(avatarUrlSigner),
            profile.Stats.Overall.ToResponse(),
            collections);
    }
}
