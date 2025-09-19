using Cortex.Mediator.Infrastructure;

namespace Application.Contracts.Persistence.Cortex.Mediator;

public interface ICustomUnitOfWork : IUnitOfWork
{
    bool HasOpenTransaction();
}