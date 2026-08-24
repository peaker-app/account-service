using Common.Application.Messaging;

namespace AccountService.Application.Collections.UpdateCollection;

public sealed record UpdateCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    string Name,
    string? Description) : ICommand;
