using AccountService.Application.Profiles.ChangeSlug;

namespace AccountService.API.Requests;

public sealed record ChangeSlugRequest(string Slug)
{
    public ChangeSlugCommand ToCommand(Guid userId) => new(userId, Slug);
}
