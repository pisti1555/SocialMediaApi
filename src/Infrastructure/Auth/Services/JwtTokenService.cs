using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Contracts.Auth;
using Application.Contracts.Services;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Services;

public sealed class JwtTokenService(IOptions<JwtConfiguration> jwtConfiguration) : ITokenService
{
    private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

    public string CreateAccessToken(string? uid, string? name, string? email, IEnumerable<string> roles, string? sid)
    {
        if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            throw new JwtException("Credentials are missing.");
        }
        
        var key = CreateSecurityKey();
        
        var claims = CreateClaims(uid, name, email, sid, roles);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        return GenerateToken(claims, credentials);
    }

    public string CreateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var randomGenerator = RandomNumberGenerator.Create();

        randomGenerator.GetBytes(randomBytes);
        
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public List<Claim> GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(token) ?? throw new JwtException("Invalid token.");
        return jwtSecurityToken.Claims.ToList();
    }

    public bool ValidateToken(string token, List<Claim> claims, bool withExpiration = true)
    {
        var jti = claims.FirstOrDefault(x => x.Type == TokenClaims.TokenId)?.Value;
        var sid = claims.FirstOrDefault(x => x.Type == TokenClaims.SessionId)?.Value;
        var nameId = claims.FirstOrDefault(x => x.Type == TokenClaims.UserId)?.Value;
        var name = claims.FirstOrDefault(x => x.Type == TokenClaims.Name)?.Value;
        var email = claims.FirstOrDefault(x => x.Type == TokenClaims.Email)?.Value;
        var roles = claims.Where(x => x.Type == TokenClaims.Role).Select(x => x.Value).ToList();
        var iat = claims.FirstOrDefault(x => x.Type == TokenClaims.IssuedAt)?.Value;
        var exp = claims.FirstOrDefault(x => x.Type == TokenClaims.Expiration)?.Value;
        var iss = claims.FirstOrDefault(x => x.Type == TokenClaims.Issuer)?.Value;
        var aud = claims.FirstOrDefault(x => x.Type == TokenClaims.Audience)?.Value;
        var sub = claims.FirstOrDefault(x => x.Type == TokenClaims.Subject)?.Value;

        if (
            string.IsNullOrWhiteSpace(jti)
            || string.IsNullOrWhiteSpace(sid)
            || string.IsNullOrWhiteSpace(nameId)
            || string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(email)
            || roles.Count == 0
            || string.IsNullOrWhiteSpace(iat)
            || string.IsNullOrWhiteSpace(exp)
            || string.IsNullOrWhiteSpace(iss)
            || string.IsNullOrWhiteSpace(aud)
            || string.IsNullOrWhiteSpace(sub)
            || !nameId.Equals(sub)
        ) return false;
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSecurityKey(),
            
            ValidateIssuer = true,
            ValidIssuer = _jwtConfiguration.Issuer,
            
            ValidateAudience = true,
            ValidAudience = _jwtConfiguration.Audience,
            
            ValidateLifetime = withExpiration,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, validationParameters, out var securityToken);
            return securityToken is not null;
        }
        catch
        {
            return false;
        }
    }


    // Helpers
    private SymmetricSecurityKey CreateSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));
    }

    private static List<Claim> CreateClaims(string uid, string name, string email, string? sid, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(TokenClaims.SessionId, sid ?? Guid.NewGuid().ToString("N")),
            new(TokenClaims.UserId, uid),
            new(TokenClaims.Name, name),
            new(TokenClaims.Email, email),
            
            new(TokenClaims.Subject, uid),
            new(TokenClaims.TokenId, Guid.NewGuid().ToString("N")),
            new(TokenClaims.IssuedAt,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(TokenClaims.Role, role)));

        return claims;
    }

    private string GenerateToken(List<Claim> claims, SigningCredentials credentials)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}