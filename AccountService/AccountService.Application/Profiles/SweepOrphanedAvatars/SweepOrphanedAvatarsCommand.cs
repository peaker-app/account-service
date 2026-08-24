using Common.Application.Messaging;

namespace AccountService.Application.Profiles.SweepOrphanedAvatars;

public sealed record SweepOrphanedAvatarsCommand(DateTime UploadedBeforeUtc) : ICommand<AvatarSweepResponse>;
