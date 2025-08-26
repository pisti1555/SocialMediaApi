using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;

namespace DomainUnitTests.PostTests.Factories;

public class PostCommentFactoryTests
{
    public static TheoryData<string?, AppUser?, Post?, bool> CommentData => new()
    {
        { "Valid comment", CreateTestUser(), CreateTestPost(), true },
        { null, CreateTestUser(), CreateTestPost(), false },
        { "", CreateTestUser(), CreateTestPost(), false },
        { "Valid comment", null, CreateTestPost(), false },
        { "Valid comment", CreateTestUser(), null, false },
        { null, null, null, false }
    };

    [Theory]
    [MemberData(nameof(CommentData))]
    public void Create_PostComment_Test(string? text, AppUser? user, Post? post, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                PostCommentFactory.Create(text!, user!, post!);
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                PostCommentFactory.Create(text!, user!, post!);
            });
        }
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
    
    private static Post CreateTestPost()
    {
        return PostFactory.Create(
            "Test text", 
            CreateTestUser()
        );
    }
}