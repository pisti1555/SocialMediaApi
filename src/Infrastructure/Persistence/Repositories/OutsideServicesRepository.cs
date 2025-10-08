using System.Linq.Expressions;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OutsideServicesRepository<T>(AppIdentityDbContext context) : IOutsideServicesRepository<T> where T : class
{
    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct) > 0;
    }
    
    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        return await context.Set<T>()
            .Where(expression)
            .FirstOrDefaultAsync(ct);
    }
}