using Domain.Common.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace API.Middlewares;

public class ErrorHandlerMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlerMiddleware> logger,
    IHostEnvironment env
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        try
        {
            await next(context);
        }
        catch (AppException e)
        {
            response.StatusCode = e.StatusCode;
            await response.WriteAsJsonAsync(
                new SimpleErrorResponse
                {
                    Status = response.StatusCode,
                    Message = e.Message
                }
            );
        }
        catch (ValidationException e)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            await response.WriteAsJsonAsync(
                new FormValidationErrorResponse
                {
                    Status = response.StatusCode,
                    Message = "Validation failed",
                    Errors = e.Errors
                        .GroupBy(err => err.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(err => err.ErrorMessage).ToList()
                        )
                }
            );
        }
        catch (Exception e)
        {
            response.StatusCode = 500;
            
            var message = env.IsDevelopment()
                ? e.Message
                : "An unexpected error occurred";
            
            await response.WriteAsJsonAsync(
                new SimpleErrorResponse
                {
                    Status = response.StatusCode,
                    Message = message
                }
            );
            
            logger.LogError(e, "Unhandled exception: {Message}", e.Message);
        }
    }
    
    private class SimpleErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private class FormValidationErrorResponse : SimpleErrorResponse
    {
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}