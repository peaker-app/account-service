using AccountService.Application.Collections.SyncCollectionPeak;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class SyncCollectionPeakCommandHandlerTests
{
    private static readonly Guid PeakId = Guid.CreateVersion7();
    private static readonly SyncCollectionPeakCommand Command = new(PeakId, "Pico de Aneto", 3410);

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SyncCollectionPeakCommandHandler _handler;

    public SyncCollectionPeakCommandHandlerTests() =>
        _handler = new SyncCollectionPeakCommandHandler(_collectionRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithAffectedCollections_UpdatesEveryDenormalisedCopy()
    {
        Collection first = CollectionFactory.WithPeak(PeakId);
        Collection second = CollectionFactory.WithPeak(PeakId);
        GivenCollectionsHolding(first, second);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        new[] { first, second }.SelectMany(collection => collection.Peaks)
            .Should().OnlyContain(peak => peak.PeakName == "Pico de Aneto" && peak.PeakAltitudeMeters == 3410);
    }

    [Fact]
    public async Task Handle_WithoutAffectedCollections_DoesNotSaveAnything()
    {
        GivenCollectionsHolding();

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTheSameNameAndAltitude_DoesNotSaveAnything()
    {
        GivenCollectionsHolding(CollectionFactory.WithPeak(PeakId));

        Result result = await _handler.Handle(Command with { PeakName = "Aneto", PeakAltitudeMeters = 3404 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithABlankPeakName_ReturnsPeakNameRequired()
    {
        GivenCollectionsHolding(CollectionFactory.WithPeak(PeakId));

        Result result = await _handler.Handle(Command with { PeakName = "  " }, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.PeakNameRequired);
    }

    private void GivenCollectionsHolding(params Collection[] collections) =>
        _collectionRepository.GetByPeakAsync(PeakId, Arg.Any<CancellationToken>())
            .Returns(collections);
}
