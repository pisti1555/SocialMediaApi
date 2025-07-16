using System;
using System.Linq;
using FluentValidation;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        var dateTime13YearsAgo = DateTime.UtcNow.AddYears(-13);
        
        RuleFor(x => x.UserName).NotNull();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.UserName).MinimumLength(3);
        RuleFor(x => x.UserName).MaximumLength(20);
        RuleFor(x => x.UserName).Must(x => !x.Any(char.IsWhiteSpace));
        
        RuleFor(x => x.Email).NotNull();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Email).MaximumLength(255);
        RuleFor(x => x.Email).Must(x => !x.Any(char.IsWhiteSpace));
        
        RuleFor(x => x.FirstName).NotNull();
        RuleFor(x => x.FirstName).NotEmpty();
        
        RuleFor(x => x.LastName).NotNull();
        RuleFor(x => x.LastName).NotEmpty();
        
        RuleFor(x => x.DateOfBirth).NotNull();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.DateOfBirth).GreaterThanOrEqualTo(DateOnly.Parse("1900-01-01"));
        RuleFor(x => x.DateOfBirth).LessThan(DateOnly.FromDateTime(dateTime13YearsAgo));
    }
}