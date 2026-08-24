using Common.Application.Messaging;

namespace AccountService.Application.Profiles.ChangeSlug;

public sealed record ChangeSlugCommand(Guid UserId, string Slug) : ICommand;
