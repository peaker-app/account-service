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
public sealed class CollectionPeakEndpointsTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task AddPeak_WithAPeakInTheCatalog_ReturnsCreatedWithTheDenormalisedData()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        Guid peakId = _factory.PeakCatalog.Register("Mont Blanc", 4808);

        using HttpResponseMessage response = await AddPeakAsync(client, collectionId, peakId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CollectionPeakResponse? peak = await response.Content.ReadFromJsonAsync<CollectionPeakResponse>();
        peak!.Should().BeEquivalentTo(new { PeakId = peakId, PeakName = "Mont Blanc", PeakAltitudeMeters = 4808 });
    }

    [Fact]
    public async Task AddPeak_TwiceInTheSameCollection_ReturnsConflict()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        Guid peakId = _factory.PeakCatalog.Register();
        (await AddPeakAsync(client, collectionId, peakId)).Dispose();

        using HttpResponseMessage response = await AddPeakAsync(client, collectionId, peakId);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddPeak_InTwoCollectionsOfTheSameHiker_IsAllowed()
    {
        using HttpClient client = await SeededClientAsync();
        Guid first = await CreateAsync(client, "Alpes");
        Guid second = await CreateAsync(client, "Pendientes");
        Guid peakId = _factory.PeakCatalog.Register();
        (await AddPeakAsync(client, first, peakId)).Dispose();

        using HttpResponseMessage response = await AddPeakAsync(client, second, peakId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddPeak_OnTheDefaultCollection_IsAllowed()
    {
        using HttpClient client = await SeededClientAsync();
        Guid defaultId = await DefaultCollectionIdAsync(client);
        Guid peakId = _factory.PeakCatalog.Register();

        using HttpResponseMessage response = await AddPeakAsync(client, defaultId, peakId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddPeak_WithAPeakOutsideTheCatalog_ReturnsNotFound()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await AddPeakAsync(client, collectionId, Guid.CreateVersion7());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddPeak_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        using HttpClient stranger = await SeededClientAsync();
        Guid collectionId = await CreateAsync(stranger, "Ajena");
        using HttpClient client = await SeededClientAsync();
        Guid peakId = _factory.PeakCatalog.Register();

        using HttpResponseMessage response = await AddPeakAsync(client, collectionId, peakId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddPeak_WhenTheCatalogIsDown_ReturnsServiceUnavailable()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        Guid peakId = _factory.PeakCatalog.Register();

        _factory.PeakCatalog.IsAvailable = false;

        try
        {
            using HttpResponseMessage response = await AddPeakAsync(client, collectionId, peakId);

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _factory.PeakCatalog.IsAvailable = true;
        }
    }

    [Fact]
    public async Task GetById_WithPeaks_PaginatesThem()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Aneto", 3404))).Dispose();
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Posets", 3375))).Dispose();

        CollectionDetailResponse? detail = await client.GetFromJsonAsync<CollectionDetailResponse>(
            $"/api/collections/{collectionId}?page=1&size=1");

        detail!.PeakCount.Should().Be(2);
        detail.Peaks.Should().BeEquivalentTo(new { Page = 1, Size = 1, TotalCount = 2, TotalPages = 2 });
        detail.Peaks.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetById_WithPeaks_ReturnsTheMostRecentlyAddedFirst()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Aneto", 3404))).Dispose();
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Posets", 3375))).Dispose();
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Monte Perdido", 3355))).Dispose();

        CollectionDetailResponse? detail =
            await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{collectionId}");

        detail!.Peaks.Items.Select(peak => peak.PeakName)
            .Should().Equal("Monte Perdido", "Posets", "Aneto");
    }

    [Fact]
    public async Task GetById_WithPeaksSpanningPages_KeepsTheNewestOnTheFirstPage()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Aneto", 3404))).Dispose();
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register("Posets", 3375))).Dispose();

        CollectionDetailResponse? detail = await client.GetFromJsonAsync<CollectionDetailResponse>(
            $"/api/collections/{collectionId}?page=1&size=1");

        detail!.Peaks.Items.Single().PeakName.Should().Be("Posets");
    }

    [Fact]
    public async Task ListMine_WithPeaks_ReportsThePeakCount()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register())).Dispose();

        PagedResponse<CollectionSummaryResponse>? page =
            await client.GetFromJsonAsync<PagedResponse<CollectionSummaryResponse>>("/api/collections");

        page!.Items.Single(item => item.Id == collectionId).PeakCount.Should().Be(1);
    }

    [Fact]
    public async Task RemovePeak_WithAPeakInTheCollection_ReturnsNoContent()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        Guid peakId = _factory.PeakCatalog.Register();
        (await AddPeakAsync(client, collectionId, peakId)).Dispose();

        using HttpResponseMessage response = await client.DeleteAsync(
            $"/api/collections/{collectionId}/peaks/{peakId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemovePeak_DoesNotRemoveItFromTheOtherCollections()
    {
        using HttpClient client = await SeededClientAsync();
        Guid first = await CreateAsync(client, "Alpes");
        Guid second = await CreateAsync(client, "Pendientes");
        Guid peakId = _factory.PeakCatalog.Register();
        (await AddPeakAsync(client, first, peakId)).Dispose();
        (await AddPeakAsync(client, second, peakId)).Dispose();

        (await client.DeleteAsync($"/api/collections/{first}/peaks/{peakId}")).Dispose();

        CollectionDetailResponse? detail =
            await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{second}");
        detail!.PeakCount.Should().Be(1);
    }

    [Fact]
    public async Task RemovePeak_WithAPeakOutsideTheCollection_ReturnsNotFound()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");

        using HttpResponseMessage response = await client.DeleteAsync(
            $"/api/collections/{collectionId}/peaks/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OnACollectionWithPeaks_CascadesThem()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        (await AddPeakAsync(client, collectionId, _factory.PeakCatalog.Register())).Dispose();

        using HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{collectionId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static Task<HttpResponseMessage> AddPeakAsync(HttpClient client, Guid collectionId, Guid peakId) =>
        client.PostAsJsonAsync($"/api/collections/{collectionId}/peaks", new { peakId });

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
