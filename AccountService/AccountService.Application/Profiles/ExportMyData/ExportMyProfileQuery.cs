using Common.Application.Messaging;

namespace AccountService.Application.Profiles.ExportMyData;

public sealed record ExportMyProfileQuery(Guid UserId) : IQuery<ProfileExportResponse>;
