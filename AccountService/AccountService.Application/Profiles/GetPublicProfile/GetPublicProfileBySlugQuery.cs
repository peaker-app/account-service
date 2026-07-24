using Common.Application.Messaging;

namespace AccountService.Application.Profiles.GetPublicProfile;

public sealed record GetPublicProfileBySlugQuery(string Slug, Guid? RequesterId)
    : IQuery<PublicProfileResponse>;
