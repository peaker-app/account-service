using Common.Domain.Abstractions;

namespace AccountService.Domain.Profiles.Events;

public sealed record ProfileUpdatedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    string DisplayName,
    string Slug,
    string Visibility) : IDomainEvent;
