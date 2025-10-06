using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Configuration;

public sealed class JwtConfigurationValidation : IValidateOptions<JwtConfiguration>
{
    private const int MinTokenKeyLength = 64;
    
    public ValidateOptionsResult Validate(string? name, JwtConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration.SecretKey))
            return ValidateOptionsResult.Fail("JWT SecretKey is not defined.");

        if (configuration.SecretKey.Length < MinTokenKeyLength)
            return ValidateOptionsResult.Fail(
                $"JWT SecretKey must be at least {MinTokenKeyLength} characters long.");

        if (string.IsNullOrWhiteSpace(configuration.Issuer))
            return ValidateOptionsResult.Fail("JWT Issuer is not defined.");

        if (string.IsNullOrWhiteSpace(configuration.Audience))
            return ValidateOptionsResult.Fail("JWT Audience is not defined.");

        if (configuration.ExpirationMinutes <= 0)
            return ValidateOptionsResult.Fail("JWT ExpirationMinutes must be greater than 0.");

        return ValidateOptionsResult.Success;
    }
}