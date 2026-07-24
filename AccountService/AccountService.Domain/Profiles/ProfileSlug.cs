using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Common.Domain.Abstractions;
using Common.Domain.Results;

namespace AccountService.Domain.Profiles;

public sealed partial class ProfileSlug : ValueObject
{
    public const int MaxLength = 80;

    private ProfileSlug(string value) => Value = value;

    public string Value { get; }

    public static Result<ProfileSlug> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ProfileErrors.SlugInvalid;
        }

        string trimmed = value.Trim();

        return trimmed.Length <= MaxLength && SlugRegex().IsMatch(trimmed)
            ? new ProfileSlug(trimmed)
            : ProfileErrors.SlugInvalid;
    }

    public static ProfileSlug FromUsername(string username)
    {
        string normalized = Normalize(username);

        return new ProfileSlug(normalized);
    }

    public ProfileSlug WithSuffix(int suffix)
    {
        string suffixText = $"-{suffix.ToString(CultureInfo.InvariantCulture)}";
        int available = MaxLength - suffixText.Length;
        string root = Value.Length > available ? Value[..available].TrimEnd('-') : Value;

        return new ProfileSlug(root + suffixText);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static string Normalize(string value)
    {
        string withoutDiacritics = RemoveDiacritics(value);

#pragma warning disable CA1308
        string lowercase = withoutDiacritics.ToLowerInvariant();
#pragma warning restore CA1308

        string hyphenated = SeparatorRegex().Replace(lowercase, "-");

        return hyphenated.Trim('-');
    }

    private static string RemoveDiacritics(string value)
    {
        string decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) is not UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SeparatorRegex();
}
