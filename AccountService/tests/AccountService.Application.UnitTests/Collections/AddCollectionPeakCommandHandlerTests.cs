using AccountService.Application.Abstractions;
using AccountService.Application.Collections.AddCollectionPeak;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class AddCollectionPeakCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid CollectionId = Guid.CreateVersion7();
    private static readonly Guid PeakId = Guid.CreateVersion7();
    private static readonly AddCollectionPeakCommand Command = new(UserId, CollectionId, PeakId);

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IPeakCatalog _peakCatalog = Substitute.For<IPeakCatalog>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly AddCollectionPeakCommandHandler _handler;

    public AddCollectionPeakCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(CollectionFactory.Now);
        _peakCatalog.GetSnapshotAsync(PeakId, Arg.Any<CancellationToken>())
            .Returns(CollectionFactory.Peak(PeakId));
        _handler = new AddCollectionPeakCommandHandler(
            _collectionRepository, _peakCatalog, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithAPeakInTheCatalog_CopiesTheDenormalisedNameAndAltitude()
    {
        GivenOwnedCollection(CollectionFactory.Custom());

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.Value.Should().BeEquivalentTo(new
        {
            PeakId,
            PeakName = "Aneto",
            PeakAltitudeMeters = 3404,
            AddedAtUtc = CollectionFactory.Now
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnTheDefaultCollection_IsAllowed()
    {
        GivenOwnedCollection(CollectionFactory.Default());

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithAPeakAlreadyInTheCollection_ReturnsPeakAlreadyAdded()
    {
        GivenOwnedCollection(CollectionFactory.WithPeak(PeakId));

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.PeakAlreadyAdded);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAPeakOutsideTheCatalog_ReturnsPeakNotFound()
    {
        GivenOwnedCollection(CollectionFactory.Custom());
        _peakCatalog.GetSnapshotAsync(PeakId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakNotFound(PeakId)));

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.PeakNotFound(PeakId));
    }

    [Fact]
    public async Task Handle_WhenTheCatalogIsDown_ReturnsPeakCatalogUnavailable()
    {
        GivenOwnedCollection(CollectionFactory.Custom());
        _peakCatalog.GetSnapshotAsync(PeakId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CollectionPeakSnapshot>(CollectionErrors.PeakCatalogUnavailable));

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.PeakCatalogUnavailable);
    }

    [Fact]
    public async Task Handle_OnACollectionOfAnotherHiker_DoesNotEvenAskTheCatalog()
    {
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns((Collection?)null);

        Result<CollectionPeakResponse> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NotFound(CollectionId));
        await _peakCatalog.DidNotReceive().GetSnapshotAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private void GivenOwnedCollection(Collection collection) =>
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns(collection);
}
