using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.ProfileAscents;

public sealed class ProfileAscent : AggregateRoot
{
    private ProfileAscent()
    {
    }

    private ProfileAscent(Guid id, ProfileAscentDraft draft) : base(id)
    {
        ProfileId = draft.ProfileId;
        AscentId = draft.AscentId;
        Peak = draft.Peak;
        AscentDate = draft.AscentDate;
        Visibility = draft.Visibility;
    }

    public Guid ProfileId { get; private set; }

    public Guid AscentId { get; private set; }

    public PeakSnapshot Peak { get; private set; } = null!;

    public DateOnly AscentDate { get; private set; }

    public AscentVisibility Visibility { get; private set; }

    public bool IsPublic => Visibility is AscentVisibility.Public;

    public static Result<ProfileAscent> Create(ProfileAscentDraft draft)
    {
        if (draft.ProfileId == Guid.Empty)
        {
            return ProfileAscentErrors.ProfileRequired;
        }

        return draft.AscentId == Guid.Empty
            ? ProfileAscentErrors.AscentRequired
            : new ProfileAscent(Guid.CreateVersion7(), draft);
    }

    public void Sync(DateOnly ascentDate, AscentVisibility visibility)
    {
        AscentDate = ascentDate;
        Visibility = visibility;
    }

    public void SyncPeak(PeakSnapshot peak) => Peak = peak;
}
