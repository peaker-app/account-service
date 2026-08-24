using AccountService.Domain.Collections;
using FluentValidation;

namespace AccountService.Application.Collections.UpdateCollection;

internal sealed class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
{
    public UpdateCollectionCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CollectionId).NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(CollectionName.MaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Collection.MaxDescriptionLength);
    }
}
