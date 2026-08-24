using FluentValidation;

namespace AccountService.Application.Profiles.UploadAvatar;

internal sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Upload.Content.Length).GreaterThan(0);
        RuleFor(command => command.Upload.FileName).NotEmpty();
    }
}
