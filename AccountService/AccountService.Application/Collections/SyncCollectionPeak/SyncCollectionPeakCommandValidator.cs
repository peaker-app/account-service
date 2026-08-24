using AccountService.Domain.Collections;
using FluentValidation;

namespace AccountService.Application.Collections.SyncCollectionPeak;

internal sealed class SyncCollectionPeakCommandValidator : AbstractValidator<SyncCollectionPeakCommand>
{
    public SyncCollectionPeakCommandValidator()
    {
        RuleFor(command => command.PeakId).NotEmpty();

        RuleFor(command => command.PeakName)
            .NotEmpty()
            .MaximumLength(CollectionPeakSnapshot.MaxNameLength);
    }
}
