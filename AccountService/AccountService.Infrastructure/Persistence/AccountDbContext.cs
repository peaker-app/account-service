using AccountService.Domain.Collections;
using AccountService.Domain.ProfileAscents;
using AccountService.Domain.Profiles;
using AccountService.Infrastructure.Persistence.Converters;
using Common.Application.Abstractions;
using Common.Infrastructure.Persistence.Idempotency;
using Common.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence;

public sealed class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<ProfileAscent> ProfileAscents => Set<ProfileAscent>();

    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Guid>().HaveColumnType("char(36)");

        // Motivo: MySql.EntityFrameworkCore no materializa DateOnly desde una columna date.
        configurationBuilder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>().HaveColumnType("date");

        configurationBuilder.Properties<DateOnly?>()
            .HaveConversion<NullableDateOnlyConverter>().HaveColumnType("date");
    }
}
