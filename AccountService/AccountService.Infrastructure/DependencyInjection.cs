using AccountService.Application.Abstractions;
using AccountService.Domain.Collections;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using AccountService.Domain.Profiles.Events;
using AccountService.Infrastructure.ExternalServices;
using AccountService.Infrastructure.Maintenance;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Messaging.Consumers;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Repositories;
using Common.Application.Abstractions;
using Common.Application.Images;
using Common.Infrastructure.Messaging;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Outbox;
using Common.Infrastructure.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AccountService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddImageStorage(configuration);
        services.AddPeakCatalog(configuration);
        services.AddEventBus(configuration, bus =>
        {
            bus.AddConsumer<UserRegisteredConsumer>();
            bus.AddConsumer<UserDeletedConsumer>();
            bus.AddConsumer<AscentRegisteredConsumer>();
            bus.AddConsumer<AscentUpdatedConsumer>();
            bus.AddConsumer<AscentDeletedConsumer>();
            bus.AddConsumer<PeakRenamedConsumer>();
            bus.AddConsumer<PeakUpdatedConsumer>().Endpoint(endpoint => endpoint.Temporary = true);
        });

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddSingleton<OutboxInterceptor>();
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));

        services.AddAccountDbContext();
        services.AddRepositories();
        services.AddHostedService<OutboxProcessor<AccountDbContext>>();
    }

    private static void AddAccountDbContext(this IServiceCollection services) =>
        services.AddDbContext<AccountDbContext>((provider, options) => options
            .UseMySQL(ResolveConnectionString(provider))
            .AddInterceptors(
                provider.GetRequiredService<AuditableEntityInterceptor>(),
                provider.GetRequiredService<OutboxInterceptor>()));

    private static string ResolveConnectionString(IServiceProvider provider)
    {
        string? connectionString = provider.GetRequiredService<IConfiguration>()
            .GetConnectionString("AccountDatabase");

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException("Connection string 'AccountDatabase' is not configured.")
            : connectionString;
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AccountDbContext>());
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IProfileAscentRepository, ProfileAscentRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<ICollectionReader, CollectionReader>();
        services.AddScoped<IDomainEventHandler<ProfileUpdatedDomainEvent>, ProfileUpdatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ProfileAvatarReplacedDomainEvent>, ProfileAvatarReplacedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ProfileAvatarStoredDomainEvent>, ProfileAvatarStoredDomainEventHandler>();
    }

    private static void AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CloudinaryOptions>()
            .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<AvatarSweepOptions>(configuration.GetSection(AvatarSweepOptions.SectionName));

        services.AddSingleton<CloudinaryFactory>();
        services.AddSingleton<IImageValidator, ImageValidator>();
        services.AddSingleton<IAvatarUrlSigner, CloudinaryAvatarUrlSigner>();
        services.AddScoped<IImageStorage, CloudinaryImageStorage>();
        services.AddScoped<IAvatarAssetInventory, CloudinaryAvatarInventory>();
        services.AddHostedService<OrphanedAvatarSweeper>();
    }

    private static void AddPeakCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PeakCatalogOptions>()
            .Bind(configuration.GetSection(PeakCatalogOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();
        services.AddHttpClient<PeakCatalogHttpClient>(ConfigurePeakCatalog)
            .AddStandardResilienceHandler();

        services.AddScoped<IPeakCatalog>(provider => new CachingPeakCatalog(
            provider.GetRequiredService<PeakCatalogHttpClient>(),
            provider.GetRequiredService<IMemoryCache>()));
    }

    private static void ConfigurePeakCatalog(IServiceProvider provider, HttpClient client)
    {
        PeakCatalogOptions options = provider.GetRequiredService<IOptions<PeakCatalogOptions>>().Value;

        client.BaseAddress = options.BaseAddress;
        client.Timeout = options.RequestTimeout;
    }
}
