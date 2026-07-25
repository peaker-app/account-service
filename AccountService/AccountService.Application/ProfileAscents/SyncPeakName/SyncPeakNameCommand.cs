using Common.Application.Messaging;

namespace AccountService.Application.ProfileAscents.SyncPeakName;

public sealed record SyncPeakNameCommand(Guid PeakId, string PeakName, int PeakAltitudeMeters) : ICommand;
