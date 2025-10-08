using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Auth.Commands.Registration;

public class RegistrationValidator : AbstractValidator<RegistrationCommand>
{
    public RegistrationValidator()
    {
        var dateTime13YearsAgo = DateTime.UtcNow.AddYears(-13);

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters.")
            .Must(x => x.All(c => char.IsLower(c) || char.IsDigit(c))).WithMessage("Username must contain only lowercase letters (a-z) and digits.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("Email cannot contain whitespace.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("Password cannot contain whitespace.")
            .Must(x => x.Any(char.IsUpper)).WithMessage("Password must contain uppercase letters.")
            .Must(x => x.Any(char.IsLower)).WithMessage("Password must contain lowercase letters.")
            .Must(x => x.Any(char.IsDigit)).WithMessage("Password must contain digits.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("First name cannot contain whitespace.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("Last name cannot contain whitespace.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .MustBeValidDate()
            .Must(x => DateOnly.TryParse(x, out var date) && date <= DateOnly.FromDateTime(DateTime.Now)).WithMessage("Date of birth must be before today.")
            .Must(x => DateOnly.TryParse(x, out var date) && date >= new DateOnly(1900, 1, 1)).WithMessage("Date of birth must be after 01/01/1900.")
            .Must(v => DateOnly.TryParse(v, out var date) && date < DateOnly.FromDateTime(dateTime13YearsAgo)).WithMessage("You must be at least 13 years old.");
    }
}