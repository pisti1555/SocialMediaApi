using Cortex.Mediator.Commands;
using FluentValidation;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TCommand, TResult>(IEnumerable<IValidator<TCommand>> validators) 
    : ICommandPipelineBehavior<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TCommand>(command);

        var validationFailures = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors);

        var failures = errors.ToList();
        
        if (failures.Count != 0)
            throw new ValidationException(failures);
        
        return await next();
    }
}