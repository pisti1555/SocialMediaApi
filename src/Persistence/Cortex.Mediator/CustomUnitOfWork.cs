using Application.Contracts.Persistence.Cortex.Mediator;
using Cortex.Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.DataContext;

namespace Persistence.Cortex.Mediator;

// Retouch Cortex.Mediator.Infrastructure.UnitOfWork
public class CustomUnitOfWork(AppDbContext context) : ICustomUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;
    
    public bool HasOpenTransaction() => _currentTransaction != null;
    private void ClearCurrentTransaction() => _currentTransaction = null;
    
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return new UnitOfWorkTransaction(_currentTransaction, this);
        
        _currentTransaction = await context.Database.BeginTransactionAsync();
        return new UnitOfWorkTransaction(_currentTransaction, this);
    }

    private class UnitOfWorkTransaction(IDbContextTransaction transaction, CustomUnitOfWork uow) : IUnitOfWorkTransaction
    {
        private bool _disposed;

        public async Task CommitAsync()
        {
            await transaction.CommitAsync();
            uow.ClearCurrentTransaction();
        }

        public async Task RollbackAsync()
        {
            await transaction.RollbackAsync();
            uow.ClearCurrentTransaction();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await transaction.DisposeAsync();
            _disposed = true;
            uow.ClearCurrentTransaction();
        }
    }
}