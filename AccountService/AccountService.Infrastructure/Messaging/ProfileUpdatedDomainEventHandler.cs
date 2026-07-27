using AccountService.Domain.Profiles.Events;
using Common.Application.Abstractions;
using Common.Contracts.Profiles;
using MassTransit;

namespace AccountService.Infrastructure.Messaging;

internal sealed class ProfileUpdatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<ProfileUpdatedDomainEvent>
{
    public Task Handle(
        ProfileUpdatedDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        publishEndpoint.Publish(
            new ProfileUpdated
            {
                MessageId = context.MessageId,
                OccurredAtUtc = context.OccurredAtUtc,
                ProfileId = domainEvent.ProfileId,
                UserId = domainEvent.UserId,
                DisplayName = domainEvent.DisplayName,
                Slug = domainEvent.Slug,
                Visibility = domainEvent.Visibility
            },
            cancellationToken);
}
