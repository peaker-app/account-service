namespace AccountService.Domain.Collections;

public sealed record CollectionDraft(Guid ProfileId, CollectionDetails Details);
