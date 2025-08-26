using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;

namespace DomainUnitTests.PostTests.Factories;

public class PostFactoryTests
{
    // Post text tests
    public static TheoryData<string, AppUser?, bool> TextTests => new()
    {
        { "This is a valid post.", CreateTestUser(), true },
        { "", CreateTestUser(), false }, // Empty
        { "   ", CreateTestUser(), false }, // Whitespace
        { new string('a', 20001), CreateTestUser(), false } // Too long
    };

    [Theory]
    [MemberData(nameof(TextTests))]
    public void Create_Post_TextValidation(string text, AppUser? user, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                PostFactory.Create(text, user!);
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                PostFactory.Create(text, user!);
            });
        }
    }
    
    public static TheoryData<string, AppUser?, bool> UserTests => new()
    {
        { "Test text", CreateTestUser(), true },
        { "Test text", null, false } // Null user
    };

    [Theory]
    [MemberData(nameof(UserTests))]
    public void Create_Post_UserValidation(string text, AppUser? user, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                PostFactory.Create(text, user!);
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                PostFactory.Create(text, user!);
            });
        }
    }

    // Bypasses data validation. Should allow entity to be created from invalid data.
    [Fact]
    public void Create_Post_BypassValidation()
    {
        var longText = new string('x', 20001);
        var user = CreateTestUser();
        
        var createdPost = PostFactory.Create(longText, user, byPassValidation: true);
        
        Assert.NotNull(createdPost);
    }

    private static AppUser CreateTestUser()
    {
        return AppUserFactory.Create(
            "TestUsername", 
            "test@email.com", 
            "Test", 
            "User", 
            DateOnly.Parse("1990-01-01")
        );
    }
}