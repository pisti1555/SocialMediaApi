using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Persistence.DataContext;

namespace Persistence.Repositories;

public class AppUserRepository(AppDbContext context, IMapper mapper) : IAppUserRepository
{
    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChangesAsync()
    {
        return context.ChangeTracker.HasChanges();   
    }
    
    public void Add(AppUser user)
    {
        context.Users.Add(user);
    }

    public void Update(AppUser user)
    {
        context.Users.Update(user);
    }

    public void Delete(AppUser user)
    {
        context.Users.Remove(user);   
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await context.Users.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<PagedResult<UserResponseDto>> GetAllDtoPagedAsync(int pageNumber, int pageSize)
    {
        var usersProjected = context.Users
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<UserResponseDto>(mapper.ConfigurationProvider);
        
        var totalCount = await usersProjected.CountAsync();
        
        var pageResultOfUsersProjected = await usersProjected
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
            
        return PagedResult<UserResponseDto>.Create(pageResultOfUsersProjected, totalCount, pageNumber, pageSize);
    }
}