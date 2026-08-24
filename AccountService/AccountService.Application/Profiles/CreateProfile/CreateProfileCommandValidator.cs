using FluentValidation;

namespace AccountService.Application.Profiles.CreateProfile;

internal sealed class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Username).NotEmpty();
    }
}
