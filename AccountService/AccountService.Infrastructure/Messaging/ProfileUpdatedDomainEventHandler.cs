using AccountService.Domain.Profiles.Events;
using Common.Application.Abstractions;
using Common.Contracts.Profiles;
using MassTransit;

namespace AccountService.Infrastructure.Messaging;

internal sealed class ProfileUpdatedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider) : IDomainEventHandler<ProfileUpdatedDomainEvent>
{
    public Task Handle(ProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(
            new ProfileUpdated
            {
                ProfileId = domainEvent.ProfileId,
                UserId = domainEvent.UserId,
                DisplayName = domainEvent.DisplayName,
                Slug = domainEvent.Slug,
                Visibility = domainEvent.Visibility,
                OccurredAtUtc = dateTimeProvider.UtcNow
            },
            cancellationToken);
}
