using Common.Application.Messaging;

namespace AccountService.Application.ProfileAscents.RemoveAscent;

public sealed record RemoveAscentCommand(Guid UserId, Guid AscentId) : ICommand;
