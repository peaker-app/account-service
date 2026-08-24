using AccountService.Application.Collections.UpdateCollection;

namespace AccountService.API.Requests;

public sealed record UpdateCollectionRequest(string Name, string? Description)
{
    public UpdateCollectionCommand ToCommand(Guid userId, Guid collectionId) =>
        new(userId, collectionId, Name, Description);
}
