using Bogus;
using Domain.Users;
using Domain.Users.Factories;

namespace IntegrationTests.Fixtures.DataFixtures;

internal static class FriendshipDataFixture
{
    internal static List<Friendship> GetFriendships(int count, AppUser? requester = null, AppUser? responder = null, bool isConfirmed = false, bool useNewSeed = true)
    {
        return GetFriendshipFaker(requester, responder, isConfirmed, useNewSeed).Generate(count);
    }
    internal static Friendship GetFriendship(AppUser? requester = null, AppUser? responder = null, bool isConfirmed = false, bool useNewSeed = true)
    {
        return GetFriendships(1, requester, responder, isConfirmed, useNewSeed)[0];
    }

    private static Faker<Friendship> GetFriendshipFaker(AppUser? requester, AppUser? responder, bool isConfirmed, bool useNewSeed)
    {
        var seed = 0;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<Friendship>()
            .CustomInstantiator(_ =>
            {
                var notNullRequester = requester ?? AppUserDataFixture.GetUser(useNewSeed);
                var notNullResponder = responder ?? AppUserDataFixture.GetUser(useNewSeed);
                
                var friendship = FriendshipFactory.Create(notNullRequester, notNullResponder);
                
                if (isConfirmed) friendship.Confirm(notNullResponder.Id);
                
                return friendship;
            }).UseSeed(seed);
    }
}