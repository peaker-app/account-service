using Common.Application.Messaging;

namespace AccountService.Application.ProfileAscents.RebuildProfileStats;

public sealed record RebuildProfileStatsCommand(Guid UserId) : ICommand<ProfileStatsRebuildResponse>;
