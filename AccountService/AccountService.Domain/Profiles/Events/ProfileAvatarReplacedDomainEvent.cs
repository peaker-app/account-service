using Common.Domain.Abstractions;

namespace AccountService.Domain.Profiles.Events;

public sealed record ProfileAvatarReplacedDomainEvent(Guid ProfileId, string PreviousPublicId) : IDomainEvent;
