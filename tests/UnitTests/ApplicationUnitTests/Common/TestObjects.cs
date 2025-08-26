using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;

namespace ApplicationUnitTests.Common;

internal static class TestObjects
{
    internal static PostComment CreateTestComment(AppUser user, Post post)
    {
        return PostCommentFactory.Create("Test comment", user, post);
    }
    
    internal static PostLike CreateTestLike(AppUser user, Post post)
    {
        return PostLikeFactory.Create(user, post);
    }
    
    internal static Post CreateTestPost(AppUser user)
    {
        return PostFactory.Create("Test post text", user);
    }
    
    internal static AppUser CreateTestUser()
    {
        return AppUserFactory.Create(
            "username",
            "test@email.com",
            "Test",
            "User",
            DateOnly.Parse("1990-01-01")
        );
    }
}