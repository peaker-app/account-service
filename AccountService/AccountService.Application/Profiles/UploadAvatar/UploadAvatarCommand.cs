using AccountService.Application.Abstractions;
using Common.Application.Messaging;

namespace AccountService.Application.Profiles.UploadAvatar;

public sealed record UploadAvatarCommand(Guid UserId, AvatarUpload Upload) : ICommand<AvatarResponse>;
