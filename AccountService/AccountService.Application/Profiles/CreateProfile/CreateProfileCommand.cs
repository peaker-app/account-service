using Common.Application.Messaging;

namespace AccountService.Application.Profiles.CreateProfile;

public sealed record CreateProfileCommand(Guid UserId, string Username) : ICommand;
