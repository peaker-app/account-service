using Common.Application.Messaging;

namespace AccountService.Application.Collections.SyncCollectionPeak;

public sealed record SyncCollectionPeakCommand(Guid PeakId, string PeakName, int PeakAltitudeMeters) : ICommand;
