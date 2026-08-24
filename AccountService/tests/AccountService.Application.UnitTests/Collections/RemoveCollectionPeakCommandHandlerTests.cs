using AccountService.Application.Collections.RemoveCollectionPeak;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class RemoveCollectionPeakCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid CollectionId = Guid.CreateVersion7();
    private static readonly Guid PeakId = Guid.CreateVersion7();
    private static readonly RemoveCollectionPeakCommand Command = new(UserId, CollectionId, PeakId);

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemoveCollectionPeakCommandHandler _handler;

    public RemoveCollectionPeakCommandHandlerTests() =>
        _handler = new RemoveCollectionPeakCommandHandler(_collectionRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithAPeakInTheCollection_DropsIt()
    {
        Collection collection = GivenOwnedCollection(CollectionFactory.WithPeak(PeakId));

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        collection.PeakCount.Should().Be(0);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAPeakOutsideTheCollection_ReturnsPeakNotInCollection()
    {
        GivenOwnedCollection(CollectionFactory.Custom());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.PeakNotInCollection(PeakId));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns((Collection?)null);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NotFound(CollectionId));
    }

    private Collection GivenOwnedCollection(Collection collection)
    {
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns(collection);

        return collection;
    }
}
