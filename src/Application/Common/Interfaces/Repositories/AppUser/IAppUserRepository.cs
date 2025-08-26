using Application.Common.Pagination;
using Application.Responses;

namespace Application.Common.Interfaces.Repositories.AppUser;

public interface IAppUserRepository : IRepositoryBase
{
    void Add(Domain.Users.AppUser user);
    void Update(Domain.Users.AppUser user);
    void Delete(Domain.Users.AppUser user);
    
    Task<Domain.Users.AppUser?> GetByIdAsync(Guid id);
    Task<Domain.Users.AppUser?> GetByUsernameAsync(string username);
    Task<Domain.Users.AppUser?> GetByEmailAsync(string email);
    
    Task<bool> ExistsByIdAsync(Guid id);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    
    
    // Public paged responses
    Task<PagedResult<UserResponseDto>> GetAllDtoPagedAsync(int pageNumber, int pageSize);
}