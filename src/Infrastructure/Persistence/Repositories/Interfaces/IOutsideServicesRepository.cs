using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories.Interfaces;

public interface IOutsideServicesRepository<T>
{
    public Task<bool> SaveChangesAsync(CancellationToken ct = default);

    public void Add(T entity);
    public void Update(T entity);
    public void Delete(T entity);
    
    public Task<T?> GetAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default);
}