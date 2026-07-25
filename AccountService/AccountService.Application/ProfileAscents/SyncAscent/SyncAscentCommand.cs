using Common.Application.Messaging;

namespace AccountService.Application.ProfileAscents.SyncAscent;

public sealed record SyncAscentCommand(
    Guid UserId,
    Guid AscentId,
    DateOnly AscentDate,
    string Visibility) : ICommand;
