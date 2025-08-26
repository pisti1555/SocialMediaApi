using Bogus;
using Domain.Users;
using Domain.Users.Factories;

namespace IntegrationTests.Fixtures.DataFixtures;

internal static class AppUserDataFixture
{
    internal static List<AppUser> GetUsers(int count, bool useNewSeed = true)
    {
        return GetUserFaker(useNewSeed).Generate(count);
    }
    internal static AppUser GetUser(bool useNewSeed = true)
    {
        return GetUsers(1, useNewSeed)[0];
    }

    private static Faker<AppUser> GetUserFaker(bool useNewSeed)
    {
        var seed = 0;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<AppUser>()
            .CustomInstantiator(faker =>
                AppUserFactory.Create(
                    faker.Internet.UserName(),
                    faker.Internet.Email(),
                    faker.Name.FirstName(),
                    faker.Name.LastName(),
                    faker.Date.BetweenDateOnly(
                        DateOnly.Parse("1950-01-01"), 
                        DateOnly.Parse("2000-01-01")
                    ),
                    true
                )
            )
            .UseSeed(seed);
    }
}