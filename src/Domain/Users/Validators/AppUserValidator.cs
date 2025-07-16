using Shared.Exceptions.CustomExceptions;

namespace Domain.Users.Validators;

internal static class AppUserValidator
{
    internal static void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new BadRequestException("User name is required.");
        if (userName.Length < 3)
            throw new BadRequestException("User name is too short.");
        if (userName.Length > 20)
            throw new BadRequestException("User name is too long.");
        if (userName.Any(char.IsWhiteSpace))
            throw new BadRequestException("User name contains whitespaces.");
    }
    
    internal static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email is required.");
        if (!email.Contains('@') || !email.Contains('.'))
            throw new BadRequestException("Email is invalid.");
        if (email.Any(char.IsWhiteSpace))
            throw new BadRequestException("Email contains whitespaces.");
        if (email.Length < 6)
            throw new BadRequestException("Email is too short.");
        if (email.Any(char.IsUpper))
            throw new BadRequestException("Email contains uppercase characters.");
    }
    
    internal static void ValidateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BadRequestException("First name is required.");
    }
    
    internal static void ValidateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new BadRequestException("Last name is required.");
    }
    
    internal static void ValidateDateOfBirth(DateOnly dateOfBirth)
    {
        if (dateOfBirth == default)
            throw new BadRequestException("Date of birth is required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Now))
            throw new BadRequestException("Date of birth is invalid.");
        if (dateOfBirth < DateOnly.FromDateTime(new DateTime(1900, 1, 1)))
            throw new BadRequestException("Date of birth is invalid.");
        if (DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)) < dateOfBirth)
            throw new BadRequestException("Minimum age is 13.");
    }
}