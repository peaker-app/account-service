using AccountService.Infrastructure.ExternalServices;
using Common.Contracts.Peaks;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace AccountService.Infrastructure.Messaging.Consumers;

internal sealed class PeakUpdatedConsumer(IMemoryCache cache) : IConsumer<PeakUpdated>
{
    public Task Consume(ConsumeContext<PeakUpdated> context)
    {
        CachingPeakCatalog.Evict(cache, context.Message.PeakId);

        return Task.CompletedTask;
    }
}
