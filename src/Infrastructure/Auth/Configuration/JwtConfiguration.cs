using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Auth.Configuration;

public sealed class JwtConfiguration
{
    [Required, MinLength(64)]
    public required string SecretKey { get; init; }
    
    [Required, Range(1, int.MaxValue)]
    public required int ExpirationMinutes { get; init; }
    
    [Required]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
}