using Common.Application.Messaging;

namespace AccountService.Application.Profiles.GetPublicProfile;

public sealed record GetPublicProfileByIdQuery(Guid TargetUserId, Guid? RequesterId)
    : IQuery<PublicProfileResponse>;
