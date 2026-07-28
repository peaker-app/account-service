using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountService.Infrastructure.Persistence;

internal sealed class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__AccountDatabase";

    private const string ModelOnlyConnectionString =
        "server=localhost;port=3308;database=peaker_accounts";

    public AccountDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<AccountDbContext> options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseMySQL(ResolveConnectionString())
            .Options;

        return new AccountDbContext(options);
    }

    private static string ResolveConnectionString()
    {
        string? configured = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        return string.IsNullOrWhiteSpace(configured)
            ? ModelOnlyConnectionString
            : configured;
    }
}
