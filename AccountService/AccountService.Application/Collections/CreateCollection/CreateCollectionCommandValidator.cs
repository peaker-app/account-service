using AccountService.Domain.Collections;
using FluentValidation;

namespace AccountService.Application.Collections.CreateCollection;

internal sealed class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(CollectionName.MaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Collection.MaxDescriptionLength);
    }
}
