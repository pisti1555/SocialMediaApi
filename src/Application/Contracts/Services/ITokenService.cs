using Application.Common.Adapters.Auth;
using Application.Common.Results;

namespace Application.Contracts.Services;

public interface ITokenService
{
    public string CreateAccessToken(string? uid, string? name, string? email, IEnumerable<string> roles, string? sid);
    public string CreateRefreshToken();
    
    public AppResult<AccessTokenClaims?> GetValidatedClaimsFromToken(string token);
    public bool ValidateToken(string token, bool withExpiration = true);
}