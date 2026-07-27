using Common.Application.Messaging;

namespace AccountService.Application.Collections.RemoveCollectionPeak;

public sealed record RemoveCollectionPeakCommand(Guid UserId, Guid CollectionId, Guid PeakId) : ICommand;
