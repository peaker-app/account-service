using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.Collections;

public sealed class CollectionName : ValueObject
{
    public const int MaxLength = 60;

    private CollectionName(string value) => Value = value;

    public string Value { get; }

    public static Result<CollectionName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return CollectionErrors.NameRequired;
        }

        string trimmed = value.Trim();

        return trimmed.Length > MaxLength
            ? CollectionErrors.NameTooLong
            : new CollectionName(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
