using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;

namespace IntegrationTests.Fixtures.DataFixtures;

public static class PostCommentDataFixture
{
    internal static PostComment GetPostComment(AppUser user, Post post, bool useNewSeed = true)
    {
        return PostCommentFactory.Create(
            "Test comment",
            user,
            post,
            true
        );
    }
}