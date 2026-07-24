using AccountService.Application.Profiles.CreateProfile;
using AccountService.Application.UnitTests.TestData;
using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Domain.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountService.Application.UnitTests.Profiles;

public sealed class CreateProfileCommandHandlerTests
{
    private static readonly CreateProfileCommand Command = new(Guid.CreateVersion7(), "Hiker_Ruben");

    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly CreateProfileCommandHandler _handler;

    public CreateProfileCommandHandlerTests()
    {
        _dateTimeProvider.UtcNow.Returns(ProfileFactory.Now);
        _handler = new CreateProfileCommandHandler(_profileRepository, _unitOfWork, _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithNewUser_CreatesProfileWithSlugFromUsername()
    {
        _profileRepository.ExistsByUserIdAsync(Command.UserId, Arg.Any<CancellationToken>()).Returns(false);
        _profileRepository.ExistsBySlugAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        Profile? added = null;
        _profileRepository.When(repository => repository.Add(Arg.Any<Profile>()))
            .Do(call => added = call.Arg<Profile>());

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added!.Slug.Value.Should().Be("hiker-ruben");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileAlreadyExists_IsIdempotentAndDoesNotCreateAnother()
    {
        _profileRepository.ExistsByUserIdAsync(Command.UserId, Arg.Any<CancellationToken>()).Returns(true);

        Result result = await _handler.Handle(Command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _profileRepository.DidNotReceive().Add(Arg.Any<Profile>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBaseSlugTaken_AppendsSuffixUntilFree()
    {
        _profileRepository.ExistsByUserIdAsync(Command.UserId, Arg.Any<CancellationToken>()).Returns(false);
        _profileRepository.ExistsBySlugAsync("hiker-ruben", Arg.Any<CancellationToken>()).Returns(true);
        _profileRepository.ExistsBySlugAsync("hiker-ruben-2", Arg.Any<CancellationToken>()).Returns(true);
        _profileRepository.ExistsBySlugAsync("hiker-ruben-3", Arg.Any<CancellationToken>()).Returns(false);
        Profile? added = null;
        _profileRepository.When(repository => repository.Add(Arg.Any<Profile>()))
            .Do(call => added = call.Arg<Profile>());

        await _handler.Handle(Command, CancellationToken.None);

        added!.Slug.Value.Should().Be("hiker-ruben-3");
    }
}
