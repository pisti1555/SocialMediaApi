using System.Linq.Expressions;
using Domain.Common;

namespace Application.Contracts.Persistence.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    // UOW
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
    bool HasChanges();
    
    // Commands
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    
    // Queries
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default);
    
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default);
    
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
}