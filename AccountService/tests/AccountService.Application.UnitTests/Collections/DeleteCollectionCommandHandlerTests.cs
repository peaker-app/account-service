using AccountService.Application.Collections.DeleteCollection;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class DeleteCollectionCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid CollectionId = Guid.CreateVersion7();
    private static readonly DeleteCollectionCommand Command = new(UserId, CollectionId);

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteCollectionCommandHandler _handler;

    public DeleteCollectionCommandHandlerTests() =>
        _handler = new DeleteCollectionCommandHandler(_collectionRepository, _unitOfWork);

    [Fact]
    public async Task Handle_OnACustomCollection_RemovesIt()
    {
        Collection collection = GivenOwnedCollection(CollectionFactory.Custom());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _collectionRepository.Received(1).Remove(collection);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnTheDefaultCollection_ReturnsDefaultNotDeletable()
    {
        GivenOwnedCollection(CollectionFactory.Default());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.DefaultNotDeletable);
    }

    [Fact]
    public async Task Handle_OnTheDefaultCollection_DoesNotRemoveAnything()
    {
        GivenOwnedCollection(CollectionFactory.Default());

        await _handler.Handle(Command, CancellationToken.None);

        _collectionRepository.DidNotReceive().Remove(Arg.Any<Collection>());
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
