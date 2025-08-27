using Application.Contracts.Persistence.Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Common.Behaviors;

public sealed class TransactionBehavior<TCommand, TResult>(ICustomUnitOfWork uow)
    : ICommandPipelineBehavior<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> Handle(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        await using var transaction = await uow.BeginTransactionAsync();
        try
        {
            var result = await next();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}