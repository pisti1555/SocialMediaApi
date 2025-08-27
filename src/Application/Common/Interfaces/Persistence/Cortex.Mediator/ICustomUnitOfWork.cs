using Cortex.Mediator.Infrastructure;

namespace Application.Common.Interfaces.Persistence.Cortex.Mediator;

public interface ICustomUnitOfWork : IUnitOfWork
{
    bool HasOpenTransaction();
}