using FluentValidation;

namespace AccountService.Application.Profiles.DeleteProfile;

internal sealed class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
{
    public DeleteProfileCommandValidator() => RuleFor(command => command.UserId).NotEmpty();
}
