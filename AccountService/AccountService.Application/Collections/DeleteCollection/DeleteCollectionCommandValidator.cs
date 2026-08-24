using FluentValidation;

namespace AccountService.Application.Collections.DeleteCollection;

internal sealed class DeleteCollectionCommandValidator : AbstractValidator<DeleteCollectionCommand>
{
    public DeleteCollectionCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CollectionId).NotEmpty();
    }
}
