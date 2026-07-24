using Xunit;

namespace AccountService.IntegrationTests;

[CollectionDefinition(nameof(AccountServiceCollection))]
public sealed class AccountServiceCollection : ICollectionFixture<AccountServiceApiFactory>;
