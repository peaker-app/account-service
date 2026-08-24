using Common.Application.Messaging;

namespace AccountService.Application.Profiles.GetMyProfile;

public sealed record GetMyProfileQuery(Guid UserId) : IQuery<ProfileResponse>;
