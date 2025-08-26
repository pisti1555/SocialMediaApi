namespace Domain.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorMessage { get; }

    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorMessage = message;
    }
}