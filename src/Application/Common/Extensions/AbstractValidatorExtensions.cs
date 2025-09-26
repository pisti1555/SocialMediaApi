using System.Globalization;
using FluentValidation;

namespace Application.Common.Extensions;

public static class AbstractValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeParsableGuid<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Invalid GUID format.");
    }
    public static IRuleBuilderOptions<T, string> MustBeValidDate<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Invalid date format.")
            .Must(x => DateOnly.TryParse(x, out var date) && date != default).WithMessage("Invalid date format.")
            .Must(BeValidDateFormat).WithMessage("Date of birth must be in YYYY-MM-DD format.");
    }
    
    private static bool BeValidDateFormat(string? date)
    {
        if (string.IsNullOrEmpty(date)) 
            return false;
        
        return DateOnly.TryParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _
        );
    }
}