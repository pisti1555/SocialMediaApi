using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Common;
using Infrastructure.Persistence.DataContext.AppDb;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class QueryRepository<TEntity, TDto>(AppDbContext context, IMapper mapper) : IRepository<TEntity, TDto> where TEntity : EntityBase
{
    public async Task<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .Where(x => x.Id == id)
            .ProjectTo<TDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<TDto?> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .Where(expression)
            .ProjectTo<TDto>(mapper.ConfigurationProvider)
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

    public async Task<List<TDto>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        return await context.Set<TEntity>()
            .Where(filter ?? (x => true))
            .ProjectTo<TDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<TDto>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        var filteredAndOrderedQuery = context.Set<TEntity>()
            .Where(filter ?? (x => true))
            .OrderByDescending(x => x.CreatedAt);
        
        var totalCount = await filteredAndOrderedQuery.CountAsync(ct);
        
        var mappedQuery = filteredAndOrderedQuery
            .ProjectTo<TDto>(mapper.ConfigurationProvider);
        
        var pageResultProjected = await mappedQuery
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync(ct);
        
        return PagedResult<TDto>.Create(pageResultProjected, totalCount, pageNumber, pageSize);
    }
}