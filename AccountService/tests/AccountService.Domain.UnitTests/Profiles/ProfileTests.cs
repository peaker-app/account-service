using AccountService.Domain.Profiles;
using AccountService.Domain.Profiles.Events;
using AccountService.Domain.UnitTests.TestData;
using Common.Domain.Results;
using FluentAssertions;
using Xunit;

namespace AccountService.Domain.UnitTests.Profiles;

public sealed class ProfileTests
{
    [Fact]
    public void Create_StartsPublicWithZeroedStats()
    {
        Profile profile = ProfileMother.Create();

        profile.Visibility.Should().Be(ProfileVisibility.Public);
        profile.Stats.Overall.Should().Be(ProfileStatsSnapshot.Empty);
        profile.Stats.Public.Should().Be(ProfileStatsSnapshot.Empty);
    }

    [Fact]
    public void RefreshStats_WithNewValues_ReplacesBothBlocksAndStampsTheUpdate()
    {
        Profile profile = ProfileMother.Create();
        ProfileStatsUpdate update = new(
            new ProfileStatsSnapshot(3, 2, 4808, Guid.CreateVersion7(), "Mont Blanc", new DateOnly(2026, 2, 9)),
            new ProfileStatsSnapshot(1, 1, 3404, Guid.CreateVersion7(), "Aneto", new DateOnly(2025, 8, 20)));
        DateTime updatedAtUtc = ProfileMother.Now.AddDays(1);

        profile.RefreshStats(update, updatedAtUtc);

        profile.Stats.Should().BeEquivalentTo(new
        {
            update.Overall,
            update.Public,
            UpdatedAtUtc = updatedAtUtc
        });
    }

    [Fact]
    public void Create_SeedsDisplayNameAndSlugFromUsername()
    {
        Profile profile = ProfileMother.Create("Hiker_Ruben");

        profile.DisplayName.Value.Should().Be("Hiker_Ruben");
        profile.Slug.Value.Should().Be("hiker-ruben");
    }

    [Fact]
    public void UpdateDetails_WithValidData_AppliesChangesAndRaisesEvent()
    {
        Profile profile = ProfileMother.Create();

        Result result = profile.UpdateDetails(ProfileMother.Details(visibility: ProfileVisibility.Private));

        result.IsSuccess.Should().BeTrue();
        profile.Visibility.Should().Be(ProfileVisibility.Private);
        profile.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ProfileUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateDetails_WithBioOverLimit_ReturnsBioTooLong()
    {
        Profile profile = ProfileMother.Create();
        string longBio = new('a', Profile.MaxBioLength + 1);

        Result result = profile.UpdateDetails(ProfileMother.Details(bio: longBio));

        result.Error.Should().Be(ProfileErrors.BioTooLong);
        profile.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeSlug_ReplacesSlugAndRaisesEvent()
    {
        Profile profile = ProfileMother.Create();
        ProfileSlug newSlug = ProfileSlug.Create("ruben-fer").Value;

        profile.ChangeSlug(newSlug);

        profile.Slug.Should().Be(newSlug);
        profile.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ProfileUpdatedDomainEvent>();
    }

    [Fact]
    public void IsVisibleTo_PrivateProfile_HidesFromOthers()
    {
        Profile profile = ProfileMother.Create();
        profile.UpdateDetails(ProfileMother.Details(visibility: ProfileVisibility.Private));

        profile.IsVisibleTo(Guid.CreateVersion7()).Should().BeFalse();
        profile.IsVisibleTo(null).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleTo_PrivateProfile_ShowsToOwner()
    {
        Profile profile = ProfileMother.Create();
        profile.UpdateDetails(ProfileMother.Details(visibility: ProfileVisibility.Private));

        profile.IsVisibleTo(profile.UserId).Should().BeTrue();
    }

    [Fact]
    public void IsVisibleTo_PublicProfile_ShowsToAnonymous()
    {
        Profile profile = ProfileMother.Create();

        profile.IsVisibleTo(null).Should().BeTrue();
    }

    [Fact]
    public void SetAvatar_WithNoPreviousAvatar_DoesNotRaiseReplacement()
    {
        Profile profile = ProfileMother.Create();

        profile.SetAvatar(new Avatar("peaker/dev/avatars/a1", "https://cdn/a1.webp"));

        profile.Avatar!.PublicId.Should().Be("peaker/dev/avatars/a1");
        profile.DomainEvents.OfType<ProfileAvatarReplacedDomainEvent>().Should().BeEmpty();
    }

    [Fact]
    public void SetAvatar_ReplacingPreviousAvatar_RaisesReplacementWithOldPublicId()
    {
        Profile profile = ProfileMother.Create();
        profile.SetAvatar(new Avatar("old-public-id", "https://cdn/old.webp"));
        profile.ClearDomainEvents();

        profile.SetAvatar(new Avatar("new-public-id", "https://cdn/new.webp"));

        profile.DomainEvents.OfType<ProfileAvatarReplacedDomainEvent>().Should().ContainSingle()
            .Which.PreviousPublicId.Should().Be("old-public-id");
        profile.Avatar!.PublicId.Should().Be("new-public-id");
    }

    [Fact]
    public void RemoveAvatar_WithPreviousAvatar_ClearsItAndRaisesReplacement()
    {
        Profile profile = ProfileMother.Create();
        profile.SetAvatar(new Avatar("old-public-id", "https://cdn/old.webp"));
        profile.ClearDomainEvents();

        profile.RemoveAvatar();

        profile.Avatar.Should().BeNull();
        profile.DomainEvents.OfType<ProfileAvatarReplacedDomainEvent>().Should().ContainSingle()
            .Which.PreviousPublicId.Should().Be("old-public-id");
    }

    [Fact]
    public void RemoveAvatar_WithNoAvatar_DoesNothing()
    {
        Profile profile = ProfileMother.Create();

        profile.RemoveAvatar();

        profile.Avatar.Should().BeNull();
        profile.DomainEvents.Should().BeEmpty();
    }
}
