using Domain.Common.Exceptions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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
            var problemDetails = new ProblemDetails
            {
                Type = env.IsDevelopment() ? e.GetType().Name : null,
                Status = e.StatusCode,
                Title = e.Title,
                Detail = e.ErrorMessage,
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };
            
            AddProblemDetailsExtensions(problemDetails, context);
            
            logger.LogError(e, "Unhandled exception occurred");
            
            response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await response.WriteAsJsonAsync(problemDetails);
        }
        catch (ValidationException e)
        {
            var errors = e.Errors
                .GroupBy(err => err.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(err => err.ErrorMessage).ToArray()
                );
            
            var validationProblemDetails = new ValidationProblemDetails
            {
                Type = env.IsDevelopment() ? e.GetType().Name : null,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = e.Message,
                Instance = $"{context.Request.Method} {context.Request.Path}",
                Errors = errors
            };
            
            AddProblemDetailsExtensions(validationProblemDetails, context);
            
            response.StatusCode = validationProblemDetails.Status ?? StatusCodes.Status400BadRequest;
            await response.WriteAsJsonAsync(validationProblemDetails);
        }
        catch (Exception e)
        {
            var problemDetails = new ProblemDetails
            {
                Type = env.IsDevelopment() ? e.GetType().Name : null,
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error occurred",
                Detail = env.IsDevelopment()
                    ? e.Message
                    : "An unexpected error occurred",
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };
            
            AddProblemDetailsExtensions(problemDetails, context);
            
            logger.LogError(e, "Unhandled exception occurred");
            
            response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
    
    private static void AddProblemDetailsExtensions(ProblemDetails problemDetails, HttpContext context)
    {
        var activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
        problemDetails.Extensions.TryAdd("traceId", activity?.Id);
        problemDetails.Extensions.TryAdd("requestId", context.TraceIdentifier);
    }
}