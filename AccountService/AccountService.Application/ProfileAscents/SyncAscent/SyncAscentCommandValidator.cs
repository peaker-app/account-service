using FluentValidation;

namespace AccountService.Application.ProfileAscents.SyncAscent;

internal sealed class SyncAscentCommandValidator : AbstractValidator<SyncAscentCommand>
{
    public SyncAscentCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AscentId).NotEmpty();
        RuleFor(command => command.AscentDate).NotEmpty();
        RuleFor(command => command.Visibility).NotEmpty();
    }
}
