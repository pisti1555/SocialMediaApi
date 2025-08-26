using FluentValidation;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        var dateTime13YearsAgo = DateTime.UtcNow.AddYears(-13);

        RuleFor(x => x.UserName)
            .NotNull().WithMessage("Username is required.")
            .NotEmpty().WithMessage("Username cannot be empty.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("Username cannot contain whitespace.");

        RuleFor(x => x.Email)
            .NotNull().WithMessage("Email is required.")
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .Must(x => !x.Any(char.IsWhiteSpace)).WithMessage("Email cannot contain whitespace.");

        RuleFor(x => x.FirstName)
            .NotNull().WithMessage("First name is required.")
            .NotEmpty().WithMessage("First name cannot be empty.");

        RuleFor(x => x.LastName)
            .NotNull().WithMessage("Last name is required.")
            .NotEmpty().WithMessage("Last name cannot be empty.");

        RuleFor(x => x.DateOfBirth)
            .NotNull().WithMessage("Date of birth is required.")
            .NotEmpty().WithMessage("Date of birth cannot be empty.")
            .GreaterThanOrEqualTo(DateOnly.Parse("1900-01-01"))
            .WithMessage("Date of birth must be after 01/01/1900.")
            .LessThan(DateOnly.FromDateTime(dateTime13YearsAgo))
            .WithMessage("You must be at least 13 years old.");
    }
}