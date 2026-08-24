using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using AccountService.Application.Abstractions;
using AccountService.Domain.Collections;
using Common.Domain.Results;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.ExternalServices;

internal sealed class PeakCatalogHttpClient(HttpClient httpClient, ILogger<PeakCatalogHttpClient> logger)
    : IPeakCatalog
{
    public async Task<Result<CollectionPeakSnapshot>> GetSnapshotAsync(
        Guid peakId,
        CancellationToken cancellationToken)
    {
        Uri route = new(string.Create(CultureInfo.InvariantCulture, $"api/peaks/{peakId}"), UriKind.Relative);

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(route, cancellationToken);

            return response.StatusCode is HttpStatusCode.NotFound
                ? Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakNotFound(peakId))
                : await ReadSnapshotAsync(response, peakId, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "The peak catalog is unreachable while resolving {PeakId}", peakId);

            return Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakCatalogUnavailable);
        }
    }

    private static async Task<Result<CollectionPeakSnapshot>> ReadSnapshotAsync(
        HttpResponseMessage response,
        Guid peakId,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakCatalogUnavailable);
        }

        PeakCatalogResource? peak =
            await response.Content.ReadFromJsonAsync<PeakCatalogResource>(cancellationToken);

        return peak is null
            ? Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakNotFound(peakId))
            : CollectionPeakSnapshot.Create(peak.Id, peak.Name, peak.AltitudeMeters);
    }
}
