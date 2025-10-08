using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contracts.Auth;
using Infrastructure.Auth.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace UnitTests.Infrastructure.Auth.JwtTokenServiceTest;

internal static class JwtTokenServiceTestHelper
{
    internal static List<Claim> CreateValidClaims(string uid, string? sub = null)
    {
        return
        [
            new Claim(TokenClaims.TokenId, Guid.NewGuid().ToString("N")),
            new Claim(TokenClaims.SessionId, Guid.NewGuid().ToString("N")),
            new Claim(TokenClaims.UserId, uid),
            new Claim(TokenClaims.Name, "test-user"),
            new Claim(TokenClaims.Email, "test@example.com"),
            new Claim(TokenClaims.Role, "User"),
            new Claim(TokenClaims.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(TokenClaims.Expiration, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(TokenClaims.Issuer, "TestIssuer"),
            new Claim(TokenClaims.Audience, "TestAudience"),
            new Claim(TokenClaims.Subject, sub ?? uid)
        ];
    }
    
    internal static string GenerateJwt(JwtConfiguration config, List<Claim> claims, DateTime? expires = null)
    {
        var now = DateTime.UtcNow;
        var expiration = expires ?? now.AddMinutes(5);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.AddMinutes(-10),
            Expires = expiration,
            SigningCredentials = credentials,
            Issuer = config.Issuer,
            Audience = config.Audience
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
    
    internal static void AssertClaimsMatch(List<Claim> claims, string type, string expected)
    {
        Assert.Equal(expected, claims.FirstOrDefault(c => c.Type == type)?.Value);
    }
    
    internal static void AssertClaimExists(List<Claim> claims, string type)
    {
        Assert.False(string.IsNullOrWhiteSpace(claims.FirstOrDefault(c => c.Type == type)?.Value));
    }
    
    internal static void AssertRolesContain(List<Claim> claims, string[] expectedRoles)
    {
        var roles = claims.Where(c => c.Type == TokenClaims.Role).Select(c => c.Value).ToList();
        foreach (var role in expectedRoles)
        {
            Assert.Contains(role, roles);
        }
    }
}