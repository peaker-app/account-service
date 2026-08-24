using AccountService.Application.ProfileAscents.RemoveAscent;
using AccountService.Infrastructure.Persistence;
using Common.Application.Abstractions;
using Common.Contracts.Ascents;
using Common.Domain.Results;
using Common.Infrastructure.Persistence.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Messaging.Consumers;

internal sealed class AscentDeletedConsumer(
    ISender sender,
    AccountDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IConsumer<AscentDeleted>
{
    public async Task Consume(ConsumeContext<AscentDeleted> context)
    {
        AscentDeleted message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        if (await IsAlreadyProcessedAsync(message.MessageId, cancellationToken))
        {
            return;
        }

        Result result = await sender.Send(
            new RemoveAscentCommand(message.UserId, message.AscentId), cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"No se pudo retirar la ascensión {message.AscentId}: {result.Error.Code}.");
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
