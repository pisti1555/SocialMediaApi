using Domain.Users.Validators;

namespace Domain.Users.Factories;

public static class AppUserFactory
{
    public static AppUser Create(
        string userName, string email, string firstName, string lastName, DateOnly dateOfBirth, bool byPassValidation = false
    )
    {
        return byPassValidation ? 
            new AppUser(userName, email, firstName, lastName, dateOfBirth) : 
            CreateWithValidation(userName, email, firstName, lastName, dateOfBirth);
    }

    private static AppUser CreateWithValidation(string userName, string email, string firstName, string lastName, DateOnly dateOfBirth)
    {
        AppUserValidator.ValidateUserName(userName);
        AppUserValidator.ValidateEmail(email);
        AppUserValidator.ValidateFirstName(firstName);
        AppUserValidator.ValidateLastName(lastName);
        AppUserValidator.ValidateDateOfBirth(dateOfBirth);

        return new AppUser(userName, email, firstName, lastName, dateOfBirth);
    }
}