using FluentValidation;

namespace AccountService.Application.ProfileAscents.RebuildProfileStats;

internal sealed class RebuildProfileStatsCommandValidator : AbstractValidator<RebuildProfileStatsCommand>
{
    public RebuildProfileStatsCommandValidator() =>
        RuleFor(command => command.UserId).NotEmpty();
}
