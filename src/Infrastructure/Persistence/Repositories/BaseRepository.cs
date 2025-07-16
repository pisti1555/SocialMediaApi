using Application.Common.Interfaces.Repositories;
using Domain.Common;
using Infrastructure.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T>(AppDbContext context) : IBaseRepository<T> where T : class, IEntityRoot
{
    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public void Delete(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        context.Set<T>().Update(entity);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var result = await context.Set<T>().FindAsync(id);
        return result != null;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await context.Set<T>().ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}