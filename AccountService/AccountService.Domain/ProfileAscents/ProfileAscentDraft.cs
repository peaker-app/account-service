namespace AccountService.Domain.ProfileAscents;

public sealed record ProfileAscentDraft(
    Guid ProfileId,
    Guid AscentId,
    PeakSnapshot Peak,
    DateOnly AscentDate,
    AscentVisibility Visibility);
