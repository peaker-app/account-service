using Common.Application.Messaging;

namespace AccountService.Application.Collections.DeleteCollection;

public sealed record DeleteCollectionCommand(Guid UserId, Guid CollectionId) : ICommand;
