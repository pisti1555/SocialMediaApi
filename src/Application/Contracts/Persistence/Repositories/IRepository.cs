using System.Linq.Expressions;
using Application.Common.Pagination;
using Domain.Common;

namespace Application.Contracts.Persistence.Repositories;

public interface IRepository<TEntity, TDto> where TEntity : EntityBase
{
    // UOW
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
    bool HasChanges();
    
    // Commands
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    
    // Queries
    Task<TEntity?> GetEntityByIdAsync(Guid id);
    Task<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default);
    
    Task<List<TDto>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
    Task<PagedResult<TDto>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
}