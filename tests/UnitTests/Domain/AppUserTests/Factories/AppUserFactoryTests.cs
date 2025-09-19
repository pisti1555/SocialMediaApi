using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users.Factories;

namespace UnitTests.Domain.AppUserTests.Factories;

public class AppUserFactoryTests
{
    // Username Tests
    public static TheoryData<string, string, string, string, DateOnly, bool> UsernameTests => new()
    {
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), true },
        { "", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { " ", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { "Test Username", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { "Te", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Less than 3 chars
        { "123456789012345678901", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // More than 20 chars
    };
    [Theory]
    [MemberData(nameof(UsernameTests))]
    public void Create_Username_ValidationTest(string userName, string email, string firstName, string lastName, DateOnly dob, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });
        }
    }
    
    // Email tests
    public static TheoryData<string, string, string, string, DateOnly, bool> EmailTests => new()
    {
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), true },
        { "TestUsername", "", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { "TestUsername", " ", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { "TestUsername", "test@user name.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Whitespace
        { "TestUsername", "t@u.c", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Less than 6 chars
        { "TestUsername", "testusername.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Missing @
        { "TestUsername", "test@usernamecom", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Missing .
        { "TestUsername", "TEST@usernamecom", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false } // Uppercase chars
    };
    [Theory]
    [MemberData(nameof(EmailTests))]
    public void Create_Email_ValidationTest(string userName, string email, string firstName, string lastName, DateOnly dob, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });
        }
    }
    
    // Name tests
    public static TheoryData<string, string, string, string, DateOnly, bool> NameTests => new()
    {
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), true },
        { "TestUsername", "test@username.com", "", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // First Whitespace
        { "TestUsername", "test@username.com", " ", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // First Whitespace
        { "TestUsername", "test@username.com", "Test", "", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Last Whitespace
        { "TestUsername", "test@username.com", "Test", " ", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), false }, // Last Whitespace
    };
    [Theory]
    [MemberData(nameof(NameTests))]
    public void Create_Firstname_ValidationTest(string userName, string email, string firstName, string lastName, DateOnly dob, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });
        }
    }
    
    // Date of Birth tests
    public static TheoryData<string, string, string, string, DateOnly, bool> DobTests => new()
    {
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), true },
        { "TestUsername", "test@username.com", "Test", "User", new DateOnly(), false }, // Default
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), false }, // Future
        { "TestUsername", "test@username.com", "Test", "User", new DateOnly(1899, 12, 31), false }, // Before 1900
        { "TestUsername", "test@username.com", "Test", "User", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-12)), false }, // Less than 13 years old
    };
    [Theory]
    [MemberData(nameof(DobTests))]
    public void Create_DateOfBirth_ValidationTest(string userName, string email, string firstName, string lastName, DateOnly dob, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                AppUserFactory.Create(
                    userName: userName,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    dateOfBirth: dob
                );
            });
        }
    }
}