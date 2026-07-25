using AccountService.Domain.ProfileAscents;
using FluentValidation;

namespace AccountService.Application.ProfileAscents.SyncPeakName;

internal sealed class SyncPeakNameCommandValidator : AbstractValidator<SyncPeakNameCommand>
{
    public SyncPeakNameCommandValidator()
    {
        RuleFor(command => command.PeakId).NotEmpty();
        RuleFor(command => command.PeakName).NotEmpty().MaximumLength(PeakSnapshot.MaxNameLength);
    }
}
