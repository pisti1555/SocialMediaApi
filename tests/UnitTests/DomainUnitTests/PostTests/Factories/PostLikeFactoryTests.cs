using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;

namespace DomainUnitTests.PostTests.Factories;

public class PostLikeFactoryTests
{
    public static TheoryData<AppUser?, Post?, bool> CreatePostLikeData => new()
    {
        { CreateTestUser(), CreateTestPost(), true },
        { null, CreateTestPost(), false },
        { CreateTestUser(), null, false },
        { null, null, false }
    };

    [Theory]
    [MemberData(nameof(CreatePostLikeData))]
    public void Create_PostLike_Test(AppUser? user, Post? post, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var exception = Record.Exception(() =>
            {
                PostLikeFactory.Create(user!, post!);
            });

            Assert.Null(exception);
        }
        else
        {
            Assert.Throws<BadRequestException>(() =>
            {
                PostLikeFactory.Create(user!, post!);
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