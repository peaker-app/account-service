using AccountService.Application.Collections.AddCollectionPeak;

namespace AccountService.API.Requests;

public sealed record AddCollectionPeakRequest(Guid PeakId)
{
    public AddCollectionPeakCommand ToCommand(Guid userId, Guid collectionId) =>
        new(userId, collectionId, PeakId);
}
