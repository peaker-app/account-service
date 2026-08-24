using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles.Events;
using Common.Application.Abstractions;

namespace AccountService.Infrastructure.Messaging;

internal sealed class ProfileAvatarReplacedDomainEventHandler(IImageStorage imageStorage)
    : IDomainEventHandler<ProfileAvatarReplacedDomainEvent>
{
    public Task Handle(
        ProfileAvatarReplacedDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        imageStorage.DeleteAsync(domainEvent.PreviousPublicId, cancellationToken);
}
