using Application.Common.Results;
using Domain.Users;

namespace Application.Contracts.Services;

public interface IAuthService
{
    public Task<AppResult> SaveTokenAsync(string jtiHash, string refreshTokenHash, string sid, string userId, bool isLongSession, CancellationToken ct = default);
    public Task<AppResult> UpdateTokenAsync(string oldRefreshTokenHash, string oldJtiHash,
        string sid, string uid, string newRefreshTokenHash, string newJtiHash, CancellationToken ct = default);
    
    public Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default);
    
    public Task<IEnumerable<string>> GetRolesAsync(AppUser user, CancellationToken ct = default);

    public Task<AppResult> CreateIdentityUserAsync(AppUser user, string password, CancellationToken ct = default);
    
    public Task DeleteIdentityUserAsync(AppUser user);
}