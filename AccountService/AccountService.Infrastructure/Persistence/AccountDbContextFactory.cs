using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountService.Infrastructure.Persistence;

internal sealed class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    private const string DesignTimeConnectionString =
        "server=localhost;port=3308;database=peaker_accounts;user=peaker;password=peaker";

    public AccountDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<AccountDbContext> options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseMySQL(DesignTimeConnectionString)
            .Options;

        return new AccountDbContext(options);
    }
}
