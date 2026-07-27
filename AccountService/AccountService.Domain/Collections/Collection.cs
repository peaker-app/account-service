using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.Collections;

public sealed class Collection : AggregateRoot
{
    public const int MaxDescriptionLength = 500;
    public const string DefaultName = "Want to climb";

    private static readonly CollectionName DefaultCollectionName = CollectionName.Create(DefaultName).Value;

    private readonly List<CollectionPeak> _peaks = [];

    private Collection()
    {
    }

    private Collection(Guid id, Guid profileId, CollectionKind kind, CollectionDetails details) : base(id)
    {
        ProfileId = profileId;
        Kind = kind;
        Name = details.Name;
        Description = details.Description;
    }

    public Guid ProfileId { get; private set; }

    public CollectionKind Kind { get; private set; }

    public CollectionName Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public IReadOnlyCollection<CollectionPeak> Peaks => [.. _peaks.OrderBy(peak => peak.AddedAtUtc)];

    public int PeakCount => _peaks.Count;

    public bool IsDefault => Kind is CollectionKind.WantToClimb;

    public static Result<Collection> CreateDefault(Guid profileId) =>
        new Collection(
            Guid.CreateVersion7(),
            profileId,
            CollectionKind.WantToClimb,
            new CollectionDetails(DefaultCollectionName, null));

    public static Result<Collection> Create(CollectionDraft draft)
    {
        Result validation = Validate(draft.Details);

        return validation.IsFailure
            ? Result.Failure<Collection>(validation.Error)
            : new Collection(Guid.CreateVersion7(), draft.ProfileId, CollectionKind.Custom, draft.Details);
    }

    public Result EnsureEditable() =>
        IsDefault ? Result.Failure(CollectionErrors.DefaultNotEditable) : Result.Success();

    public Result UpdateDetails(CollectionDetails details)
    {
        Result editable = EnsureEditable();

        if (editable.IsFailure)
        {
            return editable;
        }

        Result validation = Validate(details);

        if (validation.IsFailure)
        {
            return validation;
        }

        Name = details.Name;
        Description = details.Description;

        return Result.Success();
    }

    public Result EnsureDeletable() =>
        IsDefault ? Result.Failure(CollectionErrors.DefaultNotDeletable) : Result.Success();

    public Result<CollectionPeak> AddPeak(CollectionPeakSnapshot peak, DateTime addedAtUtc)
    {
        if (_peaks.Exists(candidate => candidate.PeakId == peak.PeakId))
        {
            return CollectionErrors.PeakAlreadyAdded;
        }

        CollectionPeak added = CollectionPeak.Create(peak, addedAtUtc);
        _peaks.Add(added);

        return added;
    }

    public Result RemovePeak(Guid peakId)
    {
        CollectionPeak? peak = _peaks.Find(candidate => candidate.PeakId == peakId);

        if (peak is null)
        {
            return Result.Failure(CollectionErrors.PeakNotInCollection(peakId));
        }

        _peaks.Remove(peak);

        return Result.Success();
    }

    public bool SyncPeak(CollectionPeakSnapshot peak)
    {
        bool changed = false;

        foreach (CollectionPeak candidate in _peaks.Where(entry => entry.PeakId == peak.PeakId))
        {
            changed |= candidate.Sync(peak);
        }

        return changed;
    }

    public bool IsOwnedBy(Guid profileId) => ProfileId == profileId;

    private static Result Validate(CollectionDetails details) =>
        details.Description is { Length: > MaxDescriptionLength }
            ? Result.Failure(CollectionErrors.DescriptionTooLong)
            : Result.Success();
}
