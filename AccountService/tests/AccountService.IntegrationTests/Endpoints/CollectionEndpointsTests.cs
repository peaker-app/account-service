using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.ListCollections;
using AccountService.Domain.Collections;
using Common.API.Responses;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class CollectionEndpointsTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task ListMine_WithoutToken_ReturnsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/collections");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListMine_OnANewProfile_ReturnsOnlyTheDefaultCollection()
    {
        using HttpClient client = await SeededClientAsync();

        PagedResponse<CollectionSummaryResponse>? page =
            await client.GetFromJsonAsync<PagedResponse<CollectionSummaryResponse>>("/api/collections");

        page!.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Name = Collection.DefaultName,
            Kind = nameof(CollectionKind.WantToClimb),
            PeakCount = 0
        });
    }

    [Fact]
    public async Task ListMine_WithSeveralCollections_PutsTheDefaultFirstAndTheRestByName()
    {
        using HttpClient client = await SeededClientAsync();
        await CreateAsync(client, "Zeta");
        await CreateAsync(client, "Alfa");

        PagedResponse<CollectionSummaryResponse>? page =
            await client.GetFromJsonAsync<PagedResponse<CollectionSummaryResponse>>("/api/collections");

        page!.Items.Select(item => item.Name).Should().Equal(Collection.DefaultName, "Alfa", "Zeta");
    }

    [Fact]
    public async Task ListMine_WithAnotherHikersCollections_NeverReturnsThem()
    {
        using HttpClient stranger = await SeededClientAsync();
        await CreateAsync(stranger, "Ajena");
        using HttpClient client = await SeededClientAsync();

        PagedResponse<CollectionSummaryResponse>? page =
            await client.GetFromJsonAsync<PagedResponse<CollectionSummaryResponse>>("/api/collections");

        page!.Items.Should().NotContain(item => item.Name == "Ajena");
    }

    [Fact]
    public async Task ListMine_WithASizeAboveTheMaximum_ReturnsBadRequest()
    {
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.GetAsync("/api/collections?page=1&size=101");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithAFreeName_ReturnsCreated()
    {
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = "Tresmiles del Pirineo", description = "Los grandes." });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithADuplicatedNameIgnoringCase_ReturnsConflict()
    {
        using HttpClient client = await SeededClientAsync();
        await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = "ALPES", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithTheDefaultName_ReturnsConflict()
    {
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = Collection.DefaultName, description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithANameUsedByAnotherHiker_IsAllowed()
    {
        using HttpClient stranger = await SeededClientAsync();
        await CreateAsync(stranger, "Compartida");
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = "Compartida", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithABlankName_ReturnsBadRequest()
    {
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = "  ", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_OnAnOwnedCollection_ReturnsItWithAnEmptyPageOfPeaks()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        CollectionDetailResponse detail =
            (await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{collectionId}"))!;

        detail.Should().BeEquivalentTo(new
        {
            Id = collectionId,
            Name = "Alpes",
            Kind = nameof(CollectionKind.Custom),
            PeakCount = 0
        });
        detail.Peaks.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        using HttpClient stranger = await SeededClientAsync();
        Guid collectionId = await CreateAsync(stranger, "Ajena");
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.GetAsync($"/api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_OnACustomCollection_PersistsTheChanges()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await client.PutJsonAsync(
            $"/api/collections/{collectionId}", new { name = "Alpes suizos", description = "Cuatromiles." });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        CollectionDetailResponse? detail =
            await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{collectionId}");
        detail!.Name.Should().Be("Alpes suizos");
    }

    [Fact]
    public async Task Update_KeepingItsOwnNameInAnotherCase_IsAllowed()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await client.PutJsonAsync(
            $"/api/collections/{collectionId}", new { name = "ALPES", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_OnTheDefaultCollection_ReturnsConflict()
    {
        using HttpClient client = await SeededClientAsync();
        Guid defaultId = await DefaultCollectionIdAsync(client);

        using HttpResponseMessage response = await client.PutJsonAsync(
            $"/api/collections/{defaultId}", new { name = "Otra", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        using HttpClient stranger = await SeededClientAsync();
        Guid collectionId = await CreateAsync(stranger, "Ajena");
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.PutJsonAsync(
            $"/api/collections/{collectionId}", new { name = "Robada", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OnACustomCollection_ReturnsNoContent()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using HttpResponseMessage lookup = await client.GetAsync($"/api/collections/{collectionId}");
        lookup.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OnTheDefaultCollection_ReturnsConflict()
    {
        using HttpClient client = await SeededClientAsync();
        Guid defaultId = await DefaultCollectionIdAsync(client);

        using HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{defaultId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        using HttpClient stranger = await SeededClientAsync();
        Guid collectionId = await CreateAsync(stranger, "Ajena");
        using HttpClient client = await SeededClientAsync();

        using HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithTheProfileAtItsCollectionLimit_ReturnsConflict()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());
        await _factory.SeedCollectionsAsync(userId, Collection.MaxPerProfile - 1);
        using HttpClient client = _factory.CreateAuthenticatedClient(userId);

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name = "Una más", description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<HttpClient> SeededClientAsync()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());

        return _factory.CreateAuthenticatedClient(userId);
    }

    private static async Task<Guid> CreateAsync(HttpClient client, string name)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/collections", new { name, description = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<Guid> DefaultCollectionIdAsync(HttpClient client)
    {
        PagedResponse<CollectionSummaryResponse>? page =
            await client.GetFromJsonAsync<PagedResponse<CollectionSummaryResponse>>("/api/collections");

        return page!.Items.Single(item => item.Kind == nameof(CollectionKind.WantToClimb)).Id;
    }
}
