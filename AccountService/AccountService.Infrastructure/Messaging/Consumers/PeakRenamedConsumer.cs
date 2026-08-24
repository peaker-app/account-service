using AccountService.Application.Collections.SyncCollectionPeak;
using AccountService.Application.ProfileAscents.SyncPeakName;
using AccountService.Infrastructure.Persistence;
using Common.Application.Abstractions;
using Common.Contracts.Peaks;
using Common.Domain.Results;
using Common.Infrastructure.Persistence.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Messaging.Consumers;

internal sealed class PeakRenamedConsumer(
    ISender sender,
    AccountDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IConsumer<PeakRenamed>
{
    public async Task Consume(ConsumeContext<PeakRenamed> context)
    {
        PeakRenamed message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        if (await IsAlreadyProcessedAsync(message.MessageId, cancellationToken))
        {
            return;
        }

        await SyncAscentsAsync(message, cancellationToken);
        await SyncCollectionsAsync(message, cancellationToken);

        dbContext.Set<ProcessedMessage>().Add(new ProcessedMessage
        {
            MessageId = message.MessageId,
            ProcessedAtUtc = dateTimeProvider.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncAscentsAsync(PeakRenamed message, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new SyncPeakNameCommand(message.PeakId, message.Name, message.AltitudeM), cancellationToken);

        EnsureSucceeded(result, message.PeakId);
    }

    private async Task SyncCollectionsAsync(PeakRenamed message, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new SyncCollectionPeakCommand(message.PeakId, message.Name, message.AltitudeM), cancellationToken);

        EnsureSucceeded(result, message.PeakId);
    }

    private static void EnsureSucceeded(Result result, Guid peakId)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"No se pudo sincronizar el nombre del pico {peakId}: {result.Error.Code}.");
        }
    }

    private Task<bool> IsAlreadyProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        dbContext.Set<ProcessedMessage>().AnyAsync(message => message.MessageId == messageId, cancellationToken);
}
