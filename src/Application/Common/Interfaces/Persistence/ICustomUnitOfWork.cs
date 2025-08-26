using Cortex.Mediator.Infrastructure;

namespace Application.Common.Interfaces.Persistence;

public interface ICustomUnitOfWork : IUnitOfWork
{
    bool HasOpenTransaction();
}