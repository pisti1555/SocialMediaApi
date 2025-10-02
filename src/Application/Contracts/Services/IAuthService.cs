using Domain.Users;

namespace Application.Contracts.Services;

public interface IAuthService
{
    public Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default);

    public Task<IdentityUserCreationResult> CreateIdentityUserFromAppUserAsync(AppUser user, string password,
        CancellationToken ct = default);
    
    public Task DeleteIdentityUserAsync(AppUser user);

    public Task<IEnumerable<string>> GetRolesAsync(AppUser user, CancellationToken ct = default);
}