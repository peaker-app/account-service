using AccountService.Application.Collections.CreateCollection;

namespace AccountService.API.Requests;

public sealed record CreateCollectionRequest(string Name, string? Description)
{
    public CreateCollectionCommand ToCommand(Guid userId) => new(userId, Name, Description);
}
