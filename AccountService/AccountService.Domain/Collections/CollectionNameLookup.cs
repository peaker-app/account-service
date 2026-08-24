namespace AccountService.Domain.Collections;

public sealed record CollectionNameLookup(Guid ProfileId, CollectionName Name, Guid? ExcludedCollectionId = null);
