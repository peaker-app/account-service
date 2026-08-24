namespace AccountService.Application.Profiles.SweepOrphanedAvatars;

public sealed record AvatarSweepResponse(int QuarantinedRemoved, int UnreferencedRemoved);
