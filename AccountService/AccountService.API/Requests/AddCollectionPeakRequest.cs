using System.Text.Json.Serialization;
using AccountService.Application.Collections.AddCollectionPeak;

namespace AccountService.API.Requests;

public sealed record AddCollectionPeakRequest([property: JsonRequired] Guid PeakId)
{
    public AddCollectionPeakCommand ToCommand(Guid userId, Guid collectionId) =>
        new(userId, collectionId, PeakId);
}
