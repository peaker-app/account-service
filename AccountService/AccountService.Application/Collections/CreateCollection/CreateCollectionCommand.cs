using Common.Application.Messaging;

namespace AccountService.Application.Collections.CreateCollection;

public sealed record CreateCollectionCommand(Guid UserId, string Name, string? Description) : ICommand<Guid>;
