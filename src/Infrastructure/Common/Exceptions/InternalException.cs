using Microsoft.AspNetCore.Http;

namespace Infrastructure.Common.Exceptions;

public abstract class InternalException : Exception
{
    public int Status { get; }
    public string Title { get; }
    public string ErrorMessage { get; }

    internal InternalException(string title, string message) : base(message)
    {
        Status = StatusCodes.Status500InternalServerError;
        Title = title;
        ErrorMessage = message;
    }
}