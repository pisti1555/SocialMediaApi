using Application.Common.Results;
using Domain.Users;

namespace Application.Contracts.Services;

public interface IAuthService
{
    public Task SaveTokenAsync(string accessToken, string refreshToken, bool isLongSession, CancellationToken ct = default);
    public Task UpdateTokenAsync(string? oldRefreshToken, string newRefreshToken,
        string? sid, string? uid, string? oldJti, string? newJti, CancellationToken ct = default);
    
    public Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default);
    
    public Task<IEnumerable<string>> GetRolesAsync(AppUser user, CancellationToken ct = default);

    public Task<AppResult> CreateIdentityUserFromAppUserAsync(AppUser user, string password,
        CancellationToken ct = default);
    
    public Task DeleteIdentityUserAsync(AppUser user);
}