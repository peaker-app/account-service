using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles.Events;
using Common.Application.Abstractions;

namespace AccountService.Infrastructure.Messaging;

internal sealed class ProfileAvatarStoredDomainEventHandler(IImageStorage imageStorage)
    : IDomainEventHandler<ProfileAvatarStoredDomainEvent>
{
    public Task Handle(
        ProfileAvatarStoredDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        imageStorage.ConfirmAsync(domainEvent.PublicId, cancellationToken);
}
