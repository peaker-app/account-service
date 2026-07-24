using FluentValidation;

namespace AccountService.Application.Profiles.RemoveAvatar;

internal sealed class RemoveAvatarCommandValidator : AbstractValidator<RemoveAvatarCommand>
{
    public RemoveAvatarCommandValidator() => RuleFor(command => command.UserId).NotEmpty();
}
