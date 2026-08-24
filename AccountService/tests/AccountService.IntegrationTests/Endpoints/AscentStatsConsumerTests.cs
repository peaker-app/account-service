using AccountService.Domain.Profiles;
using AccountService.IntegrationTests.TestData;
using Common.Contracts.Ascents;
using FluentAssertions;
using Xunit;

namespace AccountService.IntegrationTests.Endpoints;

[Collection(nameof(AccountServiceCollection))]
public sealed class AscentStatsConsumerTests(AccountServiceApiFactory factory)
{
    private readonly AccountServiceApiFactory _factory = factory;

    [Fact]
    public async Task Consume_AscentRegistered_ProjectsTheAscentAndRefreshesTheStats()
    {
        Guid userId = await SeededProfileAsync();
        Guid peakId = Guid.CreateVersion7();

        await _factory.PublishAsync(
            AscentEvents.Registered(userId, Guid.CreateVersion7(), peakId, 3404));

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);
        stats!.Overall.Should().BeEquivalentTo(new
        {
            TotalAscents = 1,
            DistinctPeaks = 1,
            HighestAltitudeMeters = 3404,
            HighestPeakId = peakId,
            LastAscentDate = new DateOnly(2025, 7, 14)
        });
    }

    [Fact]
    public async Task Consume_SameAscentRegisteredTwice_DoesNotAlterTheCounters()
    {
        Guid userId = await SeededProfileAsync();
        AscentRegistered message = AscentEvents.Registered(
            userId, Guid.CreateVersion7(), Guid.CreateVersion7(), 3404);

        await _factory.PublishAsync(message);
        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);
        await _factory.PublishAsync(message);
        await Task.Delay(TimeSpan.FromSeconds(2));

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, _ => true);
        stats!.Overall.TotalAscents.Should().Be(1);
        (await _factory.CountProjectedAscentsAsync(message.AscentId)).Should().Be(1);
    }

    [Fact]
    public async Task Consume_AscentRegisteredForTheSamePeakTwice_CountsOneDistinctPeak()
    {
        Guid userId = await SeededProfileAsync();
        Guid peakId = Guid.CreateVersion7();

        await _factory.PublishAsync(AscentEvents.Registered(userId, Guid.CreateVersion7(), peakId, 3404));
        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);
        await _factory.PublishAsync(AscentEvents.Registered(userId, Guid.CreateVersion7(), peakId, 3404));

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 2);
        stats!.Overall.DistinctPeaks.Should().Be(1);
    }

    [Fact]
    public async Task Consume_AscentDeleted_RecalculatesWithoutTheDeletedAscent()
    {
        Guid userId = await SeededProfileAsync();
        Guid anetoId = Guid.CreateVersion7();
        Guid montBlancId = Guid.CreateVersion7();
        Guid highestAscentId = Guid.CreateVersion7();

        await _factory.PublishAsync(AscentEvents.Registered(userId, Guid.CreateVersion7(), anetoId, 3404));
        await _factory.PublishAsync(
            AscentEvents.Registered(userId, highestAscentId, montBlancId, 4808, peakName: "Mont Blanc"));
        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 2);

        await _factory.PublishAsync(AscentEvents.Deleted(userId, highestAscentId, montBlancId));

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);
        stats!.Overall.Should().BeEquivalentTo(new
        {
            TotalAscents = 1,
            DistinctPeaks = 1,
            HighestAltitudeMeters = 3404,
            HighestPeakId = anetoId
        });
    }

    [Fact]
    public async Task Consume_AscentUpdated_MovesTheLastAscentDate()
    {
        Guid userId = await SeededProfileAsync();
        Guid peakId = Guid.CreateVersion7();
        Guid ascentId = Guid.CreateVersion7();
        DateOnly newDate = new(2026, 6, 1);

        await _factory.PublishAsync(AscentEvents.Registered(userId, ascentId, peakId, 3404));
        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);

        await _factory.PublishAsync(AscentEvents.Updated(userId, ascentId, peakId, newDate));

        ProfileStats? stats = await _factory.WaitForStatsAsync(
            userId, stats => stats.Overall.LastAscentDate == newDate);
        stats!.Overall.LastAscentDate.Should().Be(newDate);
    }

    [Fact]
    public async Task Consume_AscentUpdatedToPrivate_DropsItFromThePublicBlock()
    {
        Guid userId = await SeededProfileAsync();
        Guid peakId = Guid.CreateVersion7();
        Guid ascentId = Guid.CreateVersion7();

        await _factory.PublishAsync(AscentEvents.Registered(userId, ascentId, peakId, 3404));
        await _factory.WaitForStatsAsync(userId, stats => stats.Public.TotalAscents == 1);

        await _factory.PublishAsync(AscentEvents.Updated(
            userId, ascentId, peakId, new DateOnly(2025, 7, 14), AscentEvents.Private));

        ProfileStats? stats = await _factory.WaitForStatsAsync(userId, stats => stats.Public.TotalAscents == 0);
        stats!.Public.Should().Be(ProfileStatsSnapshot.Empty);
        stats.Overall.TotalAscents.Should().Be(1);
    }

    [Fact]
    public async Task Consume_PeakRenamed_RefreshesTheHighestPeakName()
    {
        Guid userId = await SeededProfileAsync();
        Guid peakId = Guid.CreateVersion7();

        await _factory.PublishAsync(AscentEvents.Registered(userId, Guid.CreateVersion7(), peakId, 3404));
        await _factory.WaitForStatsAsync(userId, stats => stats.Overall.TotalAscents == 1);

        await _factory.PublishAsync(AscentEvents.Renamed(peakId, "Pico de Aneto", 3410));

        ProfileStats? stats = await _factory.WaitForStatsAsync(
            userId, stats => stats.Overall.HighestPeakName == "Pico de Aneto");
        stats!.Overall.Should().BeEquivalentTo(new
        {
            HighestPeakName = "Pico de Aneto",
            HighestAltitudeMeters = 3410
        });
    }

    private async Task<Guid> SeededProfileAsync()
    {
        Guid userId = ApiTestHelpers.NewUserId();
        await _factory.SeedProfileAsync(userId, ApiTestHelpers.UniqueUsername());

        return userId;
    }
}
