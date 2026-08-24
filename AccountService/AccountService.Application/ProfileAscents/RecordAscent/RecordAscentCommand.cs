using Common.Application.Messaging;

namespace AccountService.Application.ProfileAscents.RecordAscent;

public sealed record RecordAscentCommand(
    Guid UserId,
    Guid AscentId,
    Guid PeakId,
    string PeakName,
    int PeakAltitudeMeters,
    DateOnly AscentDate,
    string Visibility) : ICommand;
