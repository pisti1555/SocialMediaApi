using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;

namespace IntegrationTests.Fixtures.DataFixtures;

public static class PostLikeDataFixture
{
    internal static PostLike GetPostLike(AppUser user, Post post, bool useNewSeed = true)
    {
        return PostLikeFactory.Create(
            user,
            post,
            true
        );
    }
}