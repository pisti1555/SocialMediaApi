using System.Linq.Expressions;
using Application.Common.Pagination;
using Domain.Common;

namespace Application.Contracts.Persistence.Repositories;

public interface IRepository<TEntity, TDto> where TEntity : EntityBase
{
    Task<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TDto?> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default);
    
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default);
    
    Task<List<TDto>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
    Task<PagedResult<TDto>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
}