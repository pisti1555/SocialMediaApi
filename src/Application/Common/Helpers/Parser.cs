using System.Globalization;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Common.Helpers;

internal static class Parser
{
    internal static Guid ParseIdOrThrow(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        return !result ? throw new BadRequestException("Cannot parse the id.") : guid;
    }
    
    internal static DateOnly ParseDateOrThrow(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            throw new BadRequestException("Date is required.");
        
        var result = DateOnly.TryParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateOut
        );

        return !result ? throw new BadRequestException("Date of birth is not valid.") : dateOut;
    }
}