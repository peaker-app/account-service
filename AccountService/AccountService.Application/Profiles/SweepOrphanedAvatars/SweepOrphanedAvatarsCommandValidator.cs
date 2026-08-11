using FluentValidation;

namespace AccountService.Application.Profiles.SweepOrphanedAvatars;

internal sealed class SweepOrphanedAvatarsCommandValidator : AbstractValidator<SweepOrphanedAvatarsCommand>
{
    public SweepOrphanedAvatarsCommandValidator() =>
        RuleFor(command => command.UploadedBeforeUtc).NotEmpty();
}
