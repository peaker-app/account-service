using System.Net;
using AccountService.Domain.Collections;
using AccountService.Infrastructure.ExternalServices;
using AccountService.IntegrationTests.Fakes;
using Common.Domain.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AccountService.IntegrationTests.ExternalServices;

public sealed class PeakCatalogHttpClientTests
{
    private static readonly Guid PeakId = Guid.CreateVersion7();

    [Fact]
    public async Task GetSnapshotAsync_WithAKnownPeak_ReturnsTheSnapshot()
    {
        string body = $$"""{"id":"{{PeakId}}","name":"Aneto","altitudeMeters":3404}""";
        using StubHttpMessageHandler handler = StubHttpMessageHandler.Responding(HttpStatusCode.OK, body);

        Result<CollectionPeakSnapshot> result = await SendAsync(handler);

        result.Value.Should().BeEquivalentTo(new { PeakId, Name = "Aneto", AltitudeMeters = 3404 });
    }

    [Fact]
    public async Task GetSnapshotAsync_WithNotFound_ReturnsPeakNotFound()
    {
        using StubHttpMessageHandler handler = StubHttpMessageHandler.Responding(HttpStatusCode.NotFound);

        Result<CollectionPeakSnapshot> result = await SendAsync(handler);

        result.Error.Should().Be(CollectionErrors.PeakNotFound(PeakId));
    }

    [Fact]
    public async Task GetSnapshotAsync_WithServerError_ReturnsPeakCatalogUnavailable()
    {
        using StubHttpMessageHandler handler =
            StubHttpMessageHandler.Responding(HttpStatusCode.InternalServerError);

        Result<CollectionPeakSnapshot> result = await SendAsync(handler);

        result.Error.Should().Be(CollectionErrors.PeakCatalogUnavailable);
    }

    [Fact]
    public async Task GetSnapshotAsync_WhenTheHostIsUnreachable_ReturnsPeakCatalogUnavailable()
    {
        using StubHttpMessageHandler handler = StubHttpMessageHandler.Failing(new HttpRequestException("down"));

        Result<CollectionPeakSnapshot> result = await SendAsync(handler);

        result.Error.Should().Be(CollectionErrors.PeakCatalogUnavailable);
    }

    [Fact]
    public async Task GetSnapshotAsync_WhenTheRequestTimesOut_ReturnsPeakCatalogUnavailable()
    {
        using StubHttpMessageHandler handler = StubHttpMessageHandler.Failing(new TaskCanceledException("timeout"));

        Result<CollectionPeakSnapshot> result = await SendAsync(handler);

        result.Error.Should().Be(CollectionErrors.PeakCatalogUnavailable);
    }

    private static Task<Result<CollectionPeakSnapshot>> SendAsync(StubHttpMessageHandler handler)
    {
        using var httpClient = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://peak-service:8080/")
        };

        var client = new PeakCatalogHttpClient(httpClient, NullLogger<PeakCatalogHttpClient>.Instance);

        return client.GetSnapshotAsync(PeakId, CancellationToken.None);
    }
}
