using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AccountService.Infrastructure.Persistence.Converters;

internal sealed class NullableDateOnlyConverter : ValueConverter<DateOnly?, DateTime?>
{
    public NullableDateOnlyConverter()
        : base(
            date => date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : null,
            value => value.HasValue ? DateOnly.FromDateTime(value.Value) : null)
    {
    }
}
