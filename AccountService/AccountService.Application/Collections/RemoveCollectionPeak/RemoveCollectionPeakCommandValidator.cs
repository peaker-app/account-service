using FluentValidation;

namespace AccountService.Application.Collections.RemoveCollectionPeak;

internal sealed class RemoveCollectionPeakCommandValidator : AbstractValidator<RemoveCollectionPeakCommand>
{
    public RemoveCollectionPeakCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CollectionId).NotEmpty();
        RuleFor(command => command.PeakId).NotEmpty();
    }
}
