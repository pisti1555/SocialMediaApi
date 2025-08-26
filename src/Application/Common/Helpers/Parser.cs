using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Common.Helpers;

internal static class Parser
{
    internal static Guid ParseIdOrThrow(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        return !result ? throw new BadRequestException("Cannot parse the id.") : guid;
    }
}