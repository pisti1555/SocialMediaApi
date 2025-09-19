using Bogus;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;

namespace UnitTests.Factories;

internal static class TestDataFactory
{
    private static Faker<AppUser> UserFaker(bool useNewSeed = false)
    {
        var seed = 10;
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
            ).UseSeed(seed);
    }

    private static Faker<Post> PostFaker(AppUser? user, bool useNewSeed = false)
    {
        var seed = 20;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<Post>()
            .CustomInstantiator(faker =>
                PostFactory.Create(
                    faker.Lorem.Sentence(),
                    user ?? UserFaker(useNewSeed).Generate(),
                    true
                )
            ).UseSeed(seed);
    }
    
    private static Faker<PostComment> CommentFaker(AppUser? user, Post? post, bool useNewSeed = false)
    {
        var seed = 30;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        var actualUser = user ?? UserFaker(useNewSeed).Generate();
        var actualPost = post ?? PostFaker(actualUser, useNewSeed).Generate();
        
        return new Faker<PostComment>()
            .CustomInstantiator(faker =>
                PostCommentFactory.Create(
                    faker.Lorem.Sentence(), actualUser, actualPost, true
                )
            ).UseSeed(seed);
    }
    
    private static Faker<PostLike> LikeFaker(AppUser? user, Post? post, bool useNewSeed = false)
    {
        var seed = 40;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        var actualUser = user ?? UserFaker(useNewSeed).Generate();
        var actualPost = post ?? PostFaker(actualUser, useNewSeed).Generate();
        
        return new Faker<PostLike>()
            .CustomInstantiator(_ =>
                PostLikeFactory.Create(actualUser, actualPost, true)
            ).UseSeed(seed);
    }


    // --- Factories ---
    // AppUser
    internal static AppUser CreateUser(bool useNewSeed = false) => UserFaker(useNewSeed).Generate();

    internal static List<AppUser> CreateUsers(int count, bool useNewSeed = false) => UserFaker(useNewSeed).Generate(count);

    // Post
    internal static Post CreatePost(AppUser? user = null, bool useNewSeed = false) => PostFaker(user, useNewSeed).Generate();

    internal static List<Post> CreatePosts(int count, AppUser? user = null, bool useNewSeed = false) => PostFaker(user, useNewSeed).Generate(count);

    // PostComment
    internal static PostComment CreateComment(Post? post = null, AppUser? user = null, bool useNewSeed = false) => 
        CommentFaker(user, post, useNewSeed).Generate();
    internal static List<PostComment> CreateComments(int count, Post? post = null, AppUser? user = null, bool useNewSeed = false) => 
        CommentFaker(user, post, useNewSeed).Generate(count);

    // PostLike
    internal static PostLike CreateLike(Post? post = null, AppUser? user = null, bool useNewSeed = false) => 
        LikeFaker(user, post, useNewSeed).Generate();
    internal static List<PostLike> CreateLikes(int count, Post? post = null, AppUser? user = null, bool useNewSeed = false) => 
        LikeFaker(user, post, useNewSeed).Generate(count);


    // --- Helpers ---
    internal static (AppUser User, Post Post, List<PostComment> Comments, List<PostLike> Likes) 
        CreatePostWithRelations(int commentCount = 3, int likeCount = 5)
    {
        var user = CreateUser();
        var post = CreatePost(user);
        var comments = CreateComments(commentCount, post, user);
        var likes = CreateLikes(likeCount, post, user);

        return (user, post, comments, likes);
    }
}