using System.Data.Common;
using System.Globalization;
using System.Net.Http.Headers;
using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.CreateProfile;
using AccountService.Domain.Collections;
using AccountService.Domain.Profiles;
using AccountService.IntegrationTests.Fakes;
using AccountService.Infrastructure.Persistence;
using Common.Application.Abstractions;
using Common.Contracts.Users;
using Common.Domain.Results;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;
using Xunit;

namespace AccountService.IntegrationTests;

public sealed class AccountServiceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _mySql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("peaker_accounts")
        .WithUsername("peaker")
        .WithPassword("peaker")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3-management-alpine").Build();

    private readonly TestTokenSigning _tokenSigning = new();

    internal FakeImageStorage ImageStorage { get; } = new();

    internal FakeAvatarAssetInventory AvatarAssetInventory { get; } = new();

    internal FakePeakCatalog PeakCatalog { get; } = new();

    public HttpClient CreateAuthenticatedClient(Guid userId)
    {
        HttpClient client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _tokenSigning.CreateAccessToken(userId));

        return client;
    }

    public HttpClient CreateClientWithAudience(Guid userId, string audience)
    {
        HttpClient client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _tokenSigning.CreateAccessTokenForAudience(userId, audience));

        return client;
    }

    public HttpClient CreateAdminClient(Guid userId)
    {
        HttpClient client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _tokenSigning.CreateAccessToken(userId, [PeakerRoles.Admin]));

        return client;
    }

    public async Task SeedProfileAsync(Guid userId, string username)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Result result = await sender.Send(new CreateProfileCommand(userId, username));
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"No se pudo sembrar el perfil: {result.Error.Code}.");
        }
    }

    public async Task PublishUserRegisteredAsync(UserRegistered message)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await publishEndpoint.Publish(message);
    }

    public async Task PublishUserDeletedAsync(UserDeleted message)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await publishEndpoint.Publish(message);
    }

    public async Task PublishAsync<TMessage>(TMessage message)
        where TMessage : class
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await publishEndpoint.Publish(message);
    }

    public async Task<ProfileStats?> WaitForStatsAsync(Guid userId, Func<ProfileStats, bool> predicate)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            ProfileStats? stats = await ReadStatsAsync(userId);

            if (stats is not null && predicate(stats))
            {
                return stats;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return await ReadStatsAsync(userId);
    }

    public async Task<int> CountProjectedAscentsAsync(Guid ascentId)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        return await context.ProfileAscents.CountAsync(ascent => ascent.AscentId == ascentId);
    }

    private async Task<ProfileStats?> ReadStatsAsync(Guid userId)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        Profile? profile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        return profile?.Stats;
    }

    public async Task<bool> WaitForProfileRemovalAsync(Guid userId)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (await CountProfilesAsync(userId) == 0)
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return false;
    }

    public async Task<int> CountProfilesAsync(Guid userId)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        return await context.Profiles.CountAsync(profile => profile.UserId == userId);
    }

    public async Task<int> CountDefaultCollectionsAsync(Guid userId)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        return await context.Collections.CountAsync(collection =>
            collection.Kind == CollectionKind.WantToClimb
            && context.Profiles.Any(profile => profile.Id == collection.ProfileId && profile.UserId == userId));
    }

    public async Task FillCollectionAsync(Guid collectionId, int peaks)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        Collection collection = await context.Collections.FirstAsync(candidate => candidate.Id == collectionId);

        for (int index = 0; index < peaks; index++)
        {
            CollectionPeakSnapshot peak = CollectionPeakSnapshot
                .Create(Guid.CreateVersion7(), FormattableString.Invariant($"Pico {index}"), 3000).Value;

            collection.AddPeak(peak, DateTime.UtcNow);
        }

        await context.SaveChangesAsync();
    }

    public async Task SeedCollectionsAsync(Guid userId, int collections)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        Guid profileId = await context.Profiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.Id)
            .FirstAsync();

        for (int index = 0; index < collections; index++)
        {
            CollectionName name = CollectionName.Create(FormattableString.Invariant($"Sembrada {index}")).Value;

            context.Collections.Add(
                Collection.Create(new CollectionDraft(profileId, new CollectionDetails(name, null))).Value);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Exception?> TryInsertDefaultCollectionAsync(Guid userId, string name)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        Guid profileId = await context.Profiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.Id)
            .FirstAsync();

        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO collections (id, profile_id, kind, name, created_at_utc, updated_at_utc)
                 VALUES ({Guid.CreateVersion7()}, {profileId}, 'WantToClimb', {name}, NOW(6), NOW(6))
                 """);

            return null;
        }
        catch (DbException exception)
        {
            return exception;
        }
    }

    public async Task<bool> WaitForDefaultCollectionAsync(Guid userId)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (await CountDefaultCollectionsAsync(userId) > 0)
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return false;
    }

    public async Task<bool> WaitForProfileAsync(Guid userId)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (await CountProfilesAsync(userId) > 0)
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return false;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(BuildSettings()));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IImageStorage>();
            services.AddSingleton<IImageStorage>(ImageStorage);

            services.RemoveAll<IAvatarAssetInventory>();
            services.AddSingleton<IAvatarAssetInventory>(AvatarAssetInventory);

            services.RemoveAll<IPeakCatalog>();
            services.AddSingleton<IPeakCatalog>(PeakCatalog);

            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, ConfigureTestJwtBearer);
        });
    }

    private void ConfigureTestJwtBearer(JwtBearerOptions options)
    {
        options.Authority = null;
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TestTokenSigning.Issuer,
            ValidateAudience = true,
            ValidAudiences = [TestTokenSigning.Audience],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _tokenSigning.PublicKey,
            NameClaimType = "sub",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    private Dictionary<string, string?> BuildSettings()
    {
        Uri rabbitUri = new(_rabbitMq.GetConnectionString());
        string[] credentials = rabbitUri.UserInfo.Split(':');

        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:AccountDatabase"] = _mySql.GetConnectionString(),
            ["Messaging:Host"] = rabbitUri.Host,
            ["Messaging:Port"] = rabbitUri.Port.ToString(CultureInfo.InvariantCulture),
            ["Messaging:Username"] = credentials[0],
            ["Messaging:Password"] = credentials[1],
            ["Messaging:VirtualHost"] = "/",
            ["Outbox:PollingInterval"] = "00:00:01",
            ["Outbox:RetryBackoffBase"] = "00:00:01",
            ["Outbox:RetryBackoffCap"] = "00:00:01",
            ["Jwt:Issuer"] = TestTokenSigning.Issuer,
            ["Jwt:Audiences:0"] = TestTokenSigning.Audience,
            ["Cloudinary:CloudName"] = "test",
            ["Cloudinary:ApiKey"] = "test",
            ["Cloudinary:ApiSecret"] = "test",
            ["Cloudinary:AuthTokenKey"] = "00112233445566778899aabbccddeeff",
            ["AvatarSweep:Enabled"] = "false"
        };
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _mySql.StartAsync();
        await _rabbitMq.StartAsync();

        using IServiceScope scope = Services.CreateScope();
        AccountDbContext context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _tokenSigning.Dispose();
        await _mySql.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await base.DisposeAsync();
    }
}
