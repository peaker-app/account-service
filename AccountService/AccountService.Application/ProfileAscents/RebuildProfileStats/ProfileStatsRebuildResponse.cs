namespace AccountService.Application.ProfileAscents.RebuildProfileStats;

public sealed record ProfileStatsRebuildResponse(Guid UserId, int AscentsProjected);
