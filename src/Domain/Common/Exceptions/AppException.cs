namespace Domain.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public string ErrorMessage { get; }

    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = GenerateTitle(statusCode);
        ErrorMessage = message;
    }
    
    private static string GenerateTitle(int statusCode)
    {
        return statusCode switch
        {
            // 4xx - Client errors
            400 => "Bad Request",
            401 => "Unauthorized",
            402 => "Payment Required",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            407 => "Proxy Authentication Required",
            408 => "Request Timeout",
            409 => "Conflict",
            410 => "Gone",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",

            // 5xx - Server errors
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",

            // Default
            _ => "Error"
        };
    }
}