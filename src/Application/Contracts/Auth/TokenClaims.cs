namespace Application.Contracts.Auth;

public static class TokenClaims
{
    public const string TokenId = "jti";
    public const string SessionId = "sid";
    public const string IssuedAt = "iat";
    public const string Expiration = "exp";
    public const string Subject = "sub";
    public const string Issuer = "iss";
    public const string Audience = "aud";
    
    public const string UserId = "nameid";
    public const string Name = "name";
    public const string Email = "email";
    public const string Role = "role";
}