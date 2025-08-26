using Bogus;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;

namespace IntegrationTests.Fixtures.DataFixtures;

public static class PostDataFixture
{
    internal static Post GetPost(AppUser user, bool useNewSeed = true)
    {
        return PostFactory.Create(
            "Test text",
            user,
            true
        );
    }
}