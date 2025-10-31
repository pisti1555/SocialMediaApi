using System.Linq.Expressions;
using Application.Contracts.Persistence.Repositories;
using Domain.Common;
using Infrastructure.Persistence.DataContext.AppDb;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EntityRepository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct) > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
    
    public void Add(TEntity entity)
    {
        context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .Where(expression)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .AnyAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .AnyAsync(expression, ct);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .Where(filter ?? (x => true))
            .ToListAsync(ct);
    }
}