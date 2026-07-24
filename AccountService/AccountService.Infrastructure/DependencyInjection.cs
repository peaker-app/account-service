using AccountService.Application.Abstractions;
using AccountService.Domain.Profiles;
using AccountService.Domain.Profiles.Events;
using AccountService.Infrastructure.ExternalServices;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Messaging.Consumers;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Repositories;
using Common.Application.Abstractions;
using Common.Infrastructure.Messaging;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Outbox;
using Common.Infrastructure.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddImageStorage(configuration);
        services.AddEventBus(configuration, bus => bus.AddConsumer<UserRegisteredConsumer>());

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
        services.AddScoped<IDomainEventHandler<ProfileUpdatedDomainEvent>, ProfileUpdatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ProfileAvatarReplacedDomainEvent>, ProfileAvatarReplacedDomainEventHandler>();
    }

    private static void AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.AddScoped<IImageStorage, CloudinaryImageStorage>();
    }
}
