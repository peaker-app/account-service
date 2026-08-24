using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AccountService.Infrastructure.Persistence.Converters;

internal sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(
            date => date.ToDateTime(TimeOnly.MinValue),
            value => DateOnly.FromDateTime(value))
    {
    }
}
