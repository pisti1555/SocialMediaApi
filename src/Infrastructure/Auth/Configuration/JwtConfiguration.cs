namespace Infrastructure.Auth.Configuration;

public sealed class JwtConfiguration
{
    public required string SecretKey { get; set; }
    public required int ExpirationMinutes { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}