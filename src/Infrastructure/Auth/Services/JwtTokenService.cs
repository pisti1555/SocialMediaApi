using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Adapters.Auth;
using Application.Common.Results;
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
        
        var credentials = new SigningCredentials(
            key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey)), 
            algorithm: SecurityAlgorithms.HmacSha256Signature
        );

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

    public AppResult<AccessTokenClaims?> GetValidatedClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(token) ?? throw new JwtException("Invalid token.");
        var claims = jwtSecurityToken.Claims.ToList();
        
        return AccessTokenClaims.Create(claims);
    }

    public bool ValidateToken(string token, bool withExpiration = true)
    {
        var claimsResult = GetValidatedClaimsFromToken(token);
        if (!claimsResult.Succeeded) return false;
        
        var validationParameters = new TokenValidationParameters
        {
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey)),
            
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
}