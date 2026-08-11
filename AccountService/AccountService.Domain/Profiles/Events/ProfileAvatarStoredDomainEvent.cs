using Common.Domain.Abstractions;

namespace AccountService.Domain.Profiles.Events;

public sealed record ProfileAvatarStoredDomainEvent(Guid ProfileId, string PublicId) : IDomainEvent;
