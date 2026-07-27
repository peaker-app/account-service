using AccountService.Application.Collections.CreateCollection;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Collections;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Collections;

public sealed class CreateCollectionCommandHandlerTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly CreateCollectionCommand Command = new(UserId, "Tresmiles del Pirineo", "Los grandes.");

    private readonly ICollectionRepository _collectionRepository = Substitute.For<ICollectionRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCollectionCommandHandler _handler;

    public CreateCollectionCommandHandlerTests()
    {
        _profileRepository.FindIdByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(CollectionFactory.ProfileId);
        _handler = new CreateCollectionCommandHandler(_collectionRepository, _profileRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithAFreeName_CreatesACustomCollection()
    {
        Collection? added = null;
        _collectionRepository.When(repository => repository.Add(Arg.Any<Collection>()))
            .Do(call => added = call.Arg<Collection>());

        Result<Guid> result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().BeEquivalentTo(new
        {
            ProfileId = CollectionFactory.ProfileId,
            Kind = CollectionKind.Custom,
            Description = "Los grandes."
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithADuplicatedName_ReturnsNameAlreadyUsed()
    {
        _collectionRepository
            .ExistsByNameAsync(Arg.Any<CollectionNameLookup>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Result<Guid> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NameAlreadyUsed);
        _collectionRepository.DidNotReceive().Add(Arg.Any<Collection>());
    }

    [Fact]
    public async Task Handle_WithABlankName_ReturnsNameRequired()
    {
        Result<Guid> result = await _handler.Handle(Command with { Name = "   " }, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.NameRequired);
    }

    [Fact]
    public async Task Handle_WithADescriptionLongerThanTheMaximum_ReturnsDescriptionTooLong()
    {
        var command = Command with { Description = new string('a', Collection.MaxDescriptionLength + 1) };

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(CollectionErrors.DescriptionTooLong);
    }

    [Fact]
    public async Task Handle_WithoutProfile_ReturnsProfileNotFound()
    {
        _profileRepository.FindIdByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Guid?)null);

        Result<Guid> result = await _handler.Handle(Command, CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.NotFound(UserId));
    }
}
