using Common.Application.Messaging;

namespace AccountService.Application.Profiles.RemoveAvatar;

public sealed record RemoveAvatarCommand(Guid UserId) : ICommand;
