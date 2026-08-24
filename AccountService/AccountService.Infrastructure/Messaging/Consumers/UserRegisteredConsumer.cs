using AccountService.Application.Profiles.CreateProfile;
using AccountService.Infrastructure.Persistence;
using Common.Application.Abstractions;
using Common.Contracts.Users;
using Common.Domain.Results;
using Common.Infrastructure.Persistence.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Messaging.Consumers;

internal sealed class UserRegisteredConsumer(
    ISender sender,
    AccountDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IConsumer<UserRegistered>
{
    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        UserRegistered message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        if (await IsAlreadyProcessedAsync(message.MessageId, cancellationToken))
        {
            return;
        }

        Result result = await sender.Send(
            new CreateProfileCommand(message.UserId, message.Username), cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"No se pudo crear el perfil del usuario {message.UserId}: {result.Error.Code}.");
        }

        dbContext.Set<ProcessedMessage>().Add(new ProcessedMessage
        {
            MessageId = message.MessageId,
            ProcessedAtUtc = dateTimeProvider.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<bool> IsAlreadyProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        dbContext.Set<ProcessedMessage>().AnyAsync(message => message.MessageId == messageId, cancellationToken);
}
