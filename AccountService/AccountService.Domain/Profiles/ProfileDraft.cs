namespace AccountService.Domain.Profiles;

public sealed record ProfileDraft(Guid UserId, DisplayName DisplayName, ProfileSlug Slug);
