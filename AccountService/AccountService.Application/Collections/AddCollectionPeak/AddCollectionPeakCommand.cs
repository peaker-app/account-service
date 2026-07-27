using AccountService.Application.Collections.GetCollectionById;
using Common.Application.Messaging;

namespace AccountService.Application.Collections.AddCollectionPeak;

public sealed record AddCollectionPeakCommand(Guid UserId, Guid CollectionId, Guid PeakId)
    : ICommand<CollectionPeakResponse>;
