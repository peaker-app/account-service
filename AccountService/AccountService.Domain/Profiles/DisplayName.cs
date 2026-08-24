using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.Profiles;

public sealed class DisplayName : ValueObject
{
    public const int MaxLength = 60;

    private DisplayName(string value) => Value = value;

    public string Value { get; }

    public static Result<DisplayName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ProfileErrors.DisplayNameEmpty;
        }

        string trimmed = value.Trim();

        return trimmed.Length > MaxLength
            ? ProfileErrors.DisplayNameTooLong
            : new DisplayName(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
