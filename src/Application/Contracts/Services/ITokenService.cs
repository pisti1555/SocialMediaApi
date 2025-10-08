using System.Security.Claims;

namespace Application.Contracts.Services;

public interface ITokenService
{
    public string CreateAccessToken(string? uid, string? name, string? email, IEnumerable<string> roles, string? sid);
    public string CreateRefreshToken();
    
    public List<Claim> GetClaimsFromToken(string token);
    public bool ValidateToken(string token, List<Claim> claims, bool withExpiration = true);
}