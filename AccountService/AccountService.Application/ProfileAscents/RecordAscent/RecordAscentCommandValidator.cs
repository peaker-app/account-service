using AccountService.Domain.ProfileAscents;
using FluentValidation;

namespace AccountService.Application.ProfileAscents.RecordAscent;

internal sealed class RecordAscentCommandValidator : AbstractValidator<RecordAscentCommand>
{
    public RecordAscentCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AscentId).NotEmpty();
        RuleFor(command => command.PeakId).NotEmpty();
        RuleFor(command => command.PeakName).NotEmpty().MaximumLength(PeakSnapshot.MaxNameLength);
        RuleFor(command => command.AscentDate).NotEmpty();
        RuleFor(command => command.Visibility).NotEmpty();
    }
}
