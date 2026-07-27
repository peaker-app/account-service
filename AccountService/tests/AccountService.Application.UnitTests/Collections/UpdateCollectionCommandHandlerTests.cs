using AccountService.Application.Collections.UpdateCollection;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class UpdateCollectionCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid CollectionId = Guid.CreateVersion7();
    private static readonly UpdateCollectionCommand Command = new(UserId, CollectionId, "Alpes", "Cuatromiles.");

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateCollectionCommandHandler _handler;

    public UpdateCollectionCommandHandlerTests() =>
        _handler = new UpdateCollectionCommandHandler(_collectionRepository, _unitOfWork);

    [Fact]
    public async Task Handle_OnACustomCollection_ReplacesNameAndDescription()
    {
        Collection collection = GivenOwnedCollection(CollectionFactory.Custom());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        collection.Name.Value.Should().Be("Alpes");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnTheDefaultCollection_ReturnsDefaultNotEditable()
    {
        GivenOwnedCollection(CollectionFactory.Default());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.DefaultNotEditable);
    }

    [Fact]
    public async Task Handle_OnACollectionOfAnotherHiker_ReturnsNotFound()
    {
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns((Collection?)null);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NotFound(CollectionId));
    }

    [Fact]
    public async Task Handle_WithANameUsedByAnotherCollection_ReturnsNameAlreadyUsed()
    {
        GivenOwnedCollection(CollectionFactory.Custom());
        _collectionRepository
            .ExistsByNameAsync(Arg.Any<CollectionNameLookup>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NameAlreadyUsed);
    }

    [Fact]
    public async Task Handle_CheckingUniqueness_ExcludesTheCollectionBeingRenamed()
    {
        Collection collection = GivenOwnedCollection(CollectionFactory.Custom("Alpes"));

        await _handler.Handle(Command, CancellationToken.None);

        await _collectionRepository.Received(1).ExistsByNameAsync(
            Arg.Is<CollectionNameLookup>(lookup => lookup!.ExcludedCollectionId == collection.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithABlankName_ReturnsNameRequired()
    {
        Result result = await _handler.Handle(Command with { Name = "  " }, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NameRequired);
    }

    [Fact]
    public async Task Handle_WithADescriptionLongerThanTheMaximum_ReturnsDescriptionTooLong()
    {
        GivenOwnedCollection(CollectionFactory.Custom());
        var command = Command with { Description = new string('a', Collection.MaxDescriptionLength + 1) };

        Result result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.DescriptionTooLong);
    }

    private Collection GivenOwnedCollection(Collection collection)
    {
        _collectionRepository.GetOwnedByUserAsync(CollectionId, UserId, Arg.Any<CancellationToken>())
            .Returns(collection);

        return collection;
    }
}
