using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Infrastructure.Persistence.Idempotency;
using Common.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence;

public sealed class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<Guid>().HaveColumnType("char(36)");
}
