using Common.Application.Messaging;

namespace AccountService.Application.Profiles.DeleteProfile;

public sealed record DeleteProfileCommand(Guid UserId) : ICommand;
