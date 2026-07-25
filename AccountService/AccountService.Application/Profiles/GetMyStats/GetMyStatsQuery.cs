using Common.Application.Messaging;

namespace AccountService.Application.Profiles.GetMyStats;

public sealed record GetMyStatsQuery(Guid UserId) : IQuery<ProfileStatsResponse>;
