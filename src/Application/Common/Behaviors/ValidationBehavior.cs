using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors);

        var failures = errors.ToList();
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        var response = await next(cancellationToken);

        return response;
    }
}