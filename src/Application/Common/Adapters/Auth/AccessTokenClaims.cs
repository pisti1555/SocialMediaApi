using System.Security.Claims;
using Application.Common.Results;
using Application.Contracts.Auth;

namespace Application.Common.Adapters.Auth;

public sealed class AccessTokenClaims
{
    public string Sid { get; private set; }
    public string Jti { get; private set; }
    public string Uid { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public List<string> Roles { get; private set; }

    public string Sub { get; private set; }
    public string Iat { get; private set; }
    public string Exp { get; private set; }
    public string Nbf { get; private set; }
    public string Aud { get; private set; }
    public string Iss { get; private set; }
    
    private AccessTokenClaims() {}

    public static AppResult<AccessTokenClaims?> Create(List<Claim> claims)
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
        var nbf = claims.FirstOrDefault(x => x.Type == TokenClaims.NotBefore)?.Value;
        
        if (
            string.IsNullOrWhiteSpace(jti)
            || string.IsNullOrWhiteSpace(sid)
            || string.IsNullOrWhiteSpace(nameId)
            || string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(email)
            || roles.Count == 0
            || string.IsNullOrWhiteSpace(iat)
            || string.IsNullOrWhiteSpace(exp)
            || string.IsNullOrWhiteSpace(nbf)
            || string.IsNullOrWhiteSpace(iss)
            || string.IsNullOrWhiteSpace(aud)
            || string.IsNullOrWhiteSpace(sub)
            || !nameId.Equals(sub)
        ) return AppResult<AccessTokenClaims?>.Failure(["Missing or invalid claims in access token."]);
        
        var accessTokenClaims = new AccessTokenClaims
        {
            Sid = sid,
            Jti = jti,
            Uid = nameId,
            Name = name,
            Email = email,
            Roles = roles,
            Iat = iat,
            Iss = iss,
            Aud = aud,
            Exp = exp,
            Sub = sub,
            Nbf = nbf
        };
        
        return AppResult<AccessTokenClaims?>.Success(accessTokenClaims);
    }
}