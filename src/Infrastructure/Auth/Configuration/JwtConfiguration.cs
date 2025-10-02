namespace Infrastructure.Auth.Configuration;

public sealed class JwtConfiguration
{
    public required string SecretKey { get; init; }
    public required int ExpirationMinutes { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}