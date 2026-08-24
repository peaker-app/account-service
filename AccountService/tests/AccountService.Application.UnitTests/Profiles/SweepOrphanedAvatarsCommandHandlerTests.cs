using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.SweepOrphanedAvatars;
using AccountService.Domain.Profiles;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class SweepOrphanedAvatarsCommandHandlerTests
{
    private static readonly DateTime Cutoff = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly IAvatarAssetInventory _inventory = Substitute.For<IAvatarAssetInventory>();
    private readonly IImageStorage _imageStorage = Substitute.For<IImageStorage>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();

    private readonly SweepOrphanedAvatarsCommandHandler _handler;

    public SweepOrphanedAvatarsCommandHandlerTests()
    {
        GivenQuarantined();
        GivenConfirmed();
        GivenKnown();
        GivenDeletionConfirmed(true);

        _handler = new SweepOrphanedAvatarsCommandHandler(_inventory, _imageStorage, _profileRepository);
    }

    [Fact]
    public async Task Handle_WithAnAgedQuarantinedAvatar_DeletesIt()
    {
        GivenQuarantined(Aged("orphan"));

        await _handler.Handle(new SweepOrphanedAvatarsCommand(Cutoff), CancellationToken.None);

        await _imageStorage.Received(1).TryDeleteAsync("orphan", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAnAvatarUploadedAfterTheCutoff_KeepsIt()
    {
        GivenQuarantined(new StoredAsset("in-flight", Cutoff.AddMinutes(1)));

        Result<AvatarSweepResponse> result =
            await _handler.Handle(new SweepOrphanedAvatarsCommand(Cutoff), CancellationToken.None);

        result.Value.QuarantinedRemoved.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAnAvatarStillReferenced_KeepsIt()
    {
        GivenConfirmed(Aged("referenced"));
        GivenKnown("referenced");

        Result<AvatarSweepResponse> result =
            await _handler.Handle(new SweepOrphanedAvatarsCommand(Cutoff), CancellationToken.None);

        result.Value.UnreferencedRemoved.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAnUnreferencedConfirmedAvatar_DeletesIt()
    {
        GivenConfirmed(Aged("stray"));

        Result<AvatarSweepResponse> result =
            await _handler.Handle(new SweepOrphanedAvatarsCommand(Cutoff), CancellationToken.None);

        result.Value.UnreferencedRemoved.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCloudinaryDoesNotConfirmTheDeletion_DoesNotCountItAsRemoved()
    {
        GivenConfirmed(Aged("stray"));
        GivenDeletionConfirmed(false);

        Result<AvatarSweepResponse> result =
            await _handler.Handle(new SweepOrphanedAvatarsCommand(Cutoff), CancellationToken.None);

        result.Value.UnreferencedRemoved.Should().Be(0);
    }

    private static StoredAsset Aged(string publicId) => new(publicId, Cutoff.AddDays(-1));

    private void GivenDeletionConfirmed(bool confirmed) =>
        _imageStorage.TryDeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(confirmed);

    private void GivenQuarantined(params StoredAsset[] assets) =>
        _inventory.ListQuarantinedAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlyCollection<StoredAsset>>(assets);

    private void GivenConfirmed(params StoredAsset[] assets) =>
        _inventory.ListConfirmedAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlyCollection<StoredAsset>>(assets);

    private void GivenKnown(params string[] publicIds) =>
        _profileRepository
            .GetKnownAvatarPublicIdsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlySet<string>>(publicIds.ToHashSet(StringComparer.Ordinal));
}
