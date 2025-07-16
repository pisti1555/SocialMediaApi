using Application.Common.Interfaces.Repositories;
using Application.Common.Pagination;
using Application.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Users;
using Infrastructure.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AppUserRepository(AppDbContext context, IMapper mapper) : BaseRepository<AppUser>(context), IAppUserRepository
{
    public async Task<UserResponseDto?> GetDtoByUsernameAsync(string username)
    {
        return await context.Users
            .ProjectTo<UserResponseDto>(mapper.ConfigurationProvider)
            .Where(x => x.UserName == username)
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponseDto?> GetDtoByEmailAsync(string email)
    {
        return await context.Users
            .ProjectTo<UserResponseDto>(mapper.ConfigurationProvider)
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<UserResponseDto?> GetDtoByIdAsync(Guid id)
    {
        var result = await context.Users.FindAsync(id);
        return mapper.Map<UserResponseDto>(result);
    }

    public async Task<PagedResult<UserResponseDto>> GetAllDtoPagedAsync(PaginationAttributes pagination)
    {
        var totalCount = context.Users.Count();
        var result = await context.Users
            .ProjectTo<UserResponseDto>(mapper.ConfigurationProvider)
            .Skip(pagination.PageSize * (pagination.PageNumber - 1))
            .Take(pagination.PageSize)
            .ToListAsync();
        
        return PagedResult<UserResponseDto>.Create(
            result, totalCount, pagination.PageNumber, pagination.PageSize
        );
    }
}