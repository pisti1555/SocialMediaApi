using Application.Common.Pagination;
using Application.Responses;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories;

public interface IAppUserRepository : IBaseRepository<AppUser>
{
    Task<UserResponseDto?> GetDtoByUsernameAsync(string username);
    Task<UserResponseDto?> GetDtoByEmailAsync(string email);
    
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    
    Task<UserResponseDto?> GetDtoByIdAsync(Guid id);
    Task<PagedResult<UserResponseDto>> GetAllDtoPagedAsync(PaginationAttributes pagination);
}