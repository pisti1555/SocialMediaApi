using Domain.Common;

namespace Application.Common.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class, IEntityRoot
{
    void Add(T entity);
    void Delete(T entity);
    void Update(T entity);
    
    Task<bool> ExistsAsync(Guid id);
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    
    Task<bool> SaveChangesAsync();
}