using AccountService.Domain.Profiles;
using FluentValidation;

namespace AccountService.Application.Profiles.ChangeSlug;

internal sealed class ChangeSlugCommandValidator : AbstractValidator<ChangeSlugCommand>
{
    public ChangeSlugCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(ProfileSlug.MaxLength);
    }
}
