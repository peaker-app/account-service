using FluentValidation;

namespace AccountService.Application.ProfileAscents.RemoveAscent;

internal sealed class RemoveAscentCommandValidator : AbstractValidator<RemoveAscentCommand>
{
    public RemoveAscentCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AscentId).NotEmpty();
    }
}
