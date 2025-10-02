using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contracts.Services;
using Domain.Users;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Services;

public sealed class JwtTokenService(IOptions<JwtConfiguration> jwtConfiguration) : ITokenService
{
    private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

    public string CreateToken(AppUser user, IEnumerable<string> roles)
    {
        ValidateUser(user);

        var key = CreateSecurityKey();
        
        var claims = CreateClaims(user, roles);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        return GenerateToken(claims, credentials);
    }

    private static void ValidateUser(AppUser user)
    {
        if (user.Id == Guid.Empty)
            throw new JwtException("User Id must be provided.");
            
        if (string.IsNullOrWhiteSpace(user.UserName))
            throw new JwtException("Username must be provided.");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new JwtException("Email must be provided.");
    }
    
    private SymmetricSecurityKey CreateSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));
    }

    private static List<Claim> CreateClaims(AppUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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