using AccountService.Domain.Profiles;
using FluentValidation;

namespace AccountService.Application.Profiles.UpdateProfile;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MaximumLength(DisplayName.MaxLength);

        RuleFor(command => command.Bio)
            .MaximumLength(Profile.MaxBioLength);

        RuleFor(command => command.CountryCode)
            .Length(CountryCode.Length)
            .When(command => !string.IsNullOrWhiteSpace(command.CountryCode));

        RuleFor(command => command.Visibility).IsInEnum();
    }
}
