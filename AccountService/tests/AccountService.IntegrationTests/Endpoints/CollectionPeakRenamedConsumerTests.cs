using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.IntegrationTests.TestData;
using Common.Contracts.Peaks;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class CollectionPeakRenamedConsumerTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task Consume_PeakRenamed_RefreshesTheDenormalisedPeakInEveryCollection()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Alpes");
        Guid peakId = _factory.PeakCatalog.Register("Aneto", 3404);
        (await AddPeakAsync(client, collectionId, peakId)).Dispose();

        await _factory.PublishAsync(AscentEvents.Renamed(peakId, "Pico de Aneto", 3410));

        CollectionPeakResponse? peak = await WaitForPeakAsync(
            client, collectionId, candidate => candidate.PeakName == "Pico de Aneto");
        peak.Should().BeEquivalentTo(new { PeakName = "Pico de Aneto", PeakAltitudeMeters = 3410 });
    }

    [Fact]
    public async Task Consume_SameMessageTwice_LeavesASingleCopyOfThePeak()
    {
        using HttpClient client = await SeededClientAsync();
        Guid collectionId = await CreateAsync(client, "Pirineos");
        Guid peakId = _factory.PeakCatalog.Register("Posets", 3375);
        (await AddPeakAsync(client, collectionId, peakId)).Dispose();
        PeakRenamed message = AscentEvents.Renamed(peakId, "Llardana", 3380);

        await _factory.PublishAsync(message);
        await WaitForPeakAsync(client, collectionId, candidate => candidate.PeakName == "Llardana");
        await _factory.PublishAsync(message);
        await Task.Delay(TimeSpan.FromSeconds(2));

        CollectionDetailResponse? detail =
            await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{collectionId}");
        detail!.PeakCount.Should().Be(1);
        detail.Peaks.Items.Should().ContainSingle().Which.PeakName.Should().Be("Llardana");
    }

    private static async Task<CollectionPeakResponse?> WaitForPeakAsync(
        HttpClient client,
        Guid collectionId,
        Func<CollectionPeakResponse, bool> predicate)
    {
        CollectionPeakResponse? peak = null;

        for (int attempt = 0; attempt < 40 && peak is null; attempt++)
        {
            CollectionDetailResponse? detail =
                await client.GetFromJsonAsync<CollectionDetailResponse>($"/api/collections/{collectionId}");

            peak = detail!.Peaks.Items.FirstOrDefault(candidate => predicate(candidate));

            if (peak is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        return peak;
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
}
