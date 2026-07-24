using AccountService.Domain.Profiles.Events;
using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.Profiles;

public sealed class Profile : AggregateRoot
{
    public const int MaxBioLength = 500;

    private Profile()
    {
    }

    private Profile(Guid id, ProfileDraft draft, DateTime utcNow) : base(id)
    {
        UserId = draft.UserId;
        DisplayName = draft.DisplayName;
        Slug = draft.Slug;
        Visibility = ProfileVisibility.Public;
        Stats = ProfileStats.Initial(utcNow);
    }

    public Guid UserId { get; private set; }

    public DisplayName DisplayName { get; private set; } = null!;

    public ProfileSlug Slug { get; private set; } = null!;

    public string? Bio { get; private set; }

    public Avatar? Avatar { get; private set; }

    public CountryCode? CountryCode { get; private set; }

    public ProfileVisibility Visibility { get; private set; }

    public ProfileStats Stats { get; private set; } = null!;

    public static Result<Profile> Create(ProfileDraft draft, DateTime utcNow) =>
        new Profile(Guid.CreateVersion7(), draft, utcNow);

    public bool IsVisibleTo(Guid? requesterId) =>
        Visibility is ProfileVisibility.Public || requesterId == UserId;

    public Result UpdateDetails(ProfileDetails details)
    {
        if (details.Bio is { Length: > MaxBioLength })
        {
            return Result.Failure(ProfileErrors.BioTooLong);
        }

        DisplayName = details.DisplayName;
        Bio = details.Bio;
        CountryCode = details.CountryCode;
        Visibility = details.Visibility;

        RaiseUpdated();

        return Result.Success();
    }

    public void ChangeSlug(ProfileSlug slug)
    {
        Slug = slug;
        RaiseUpdated();
    }

    public void SetAvatar(Avatar avatar)
    {
        RaisePreviousAvatarReplacement();
        Avatar = avatar;
    }

    public void RemoveAvatar()
    {
        if (Avatar is null)
        {
            return;
        }

        RaisePreviousAvatarReplacement();
        Avatar = null;
    }

    private void RaisePreviousAvatarReplacement()
    {
        if (Avatar is not null)
        {
            Raise(new ProfileAvatarReplacedDomainEvent(Id, Avatar.PublicId));
        }
    }

    private void RaiseUpdated() =>
        Raise(new ProfileUpdatedDomainEvent(Id, UserId, DisplayName.Value, Slug.Value, Visibility.ToString()));
}
