using FluentValidation;

namespace AccountService.Application.Collections.AddCollectionPeak;

internal sealed class AddCollectionPeakCommandValidator : AbstractValidator<AddCollectionPeakCommand>
{
    public AddCollectionPeakCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CollectionId).NotEmpty();
        RuleFor(command => command.PeakId).NotEmpty();
    }
}
