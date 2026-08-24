using AccountService.Domain.ProfileAscents;
using AccountService.Domain.UnitTests.TestData;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.ProfileAscents;

public sealed class ProfileAscentTests
{
    [Fact]
    public void Create_WithValidDraft_ReturnsProfileAscent()
    {
        Result<ProfileAscent> result = ProfileAscent.Create(ProfileAscentMother.Draft());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutProfile_ReturnsProfileRequired()
    {
        ProfileAscentDraft draft = ProfileAscentMother.Draft() with { ProfileId = Guid.Empty };

        Result<ProfileAscent> result = ProfileAscent.Create(draft);

        result.Error.Should().Be(ProfileAscentErrors.ProfileRequired);
    }

    [Fact]
    public void Create_WithoutAscent_ReturnsAscentRequired()
    {
        ProfileAscentDraft draft = ProfileAscentMother.Draft() with { AscentId = Guid.Empty };

        Result<ProfileAscent> result = ProfileAscent.Create(draft);

        result.Error.Should().Be(ProfileAscentErrors.AscentRequired);
    }

    [Fact]
    public void Create_WithPrivateVisibility_IsNotPublic()
    {
        ProfileAscent ascent = ProfileAscentMother.Create(visibility: AscentVisibility.Private);

        ascent.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void Sync_WithNewDateAndVisibility_ReplacesBoth()
    {
        ProfileAscent ascent = ProfileAscentMother.Create();
        DateOnly newDate = new(2026, 3, 1);

        ascent.Sync(newDate, AscentVisibility.Private);

        ascent.Should().BeEquivalentTo(new { AscentDate = newDate, Visibility = AscentVisibility.Private });
    }

    [Fact]
    public void SyncPeak_WithRenamedPeak_ReplacesTheDenormalisedSnapshot()
    {
        ProfileAscent ascent = ProfileAscentMother.Create();
        PeakSnapshot renamed = ProfileAscentMother.Peak(ProfileAscentMother.AnetoId, "Pico de Aneto", 3410);

        ascent.SyncPeak(renamed);

        ascent.Peak.Should().Be(renamed);
    }
}
