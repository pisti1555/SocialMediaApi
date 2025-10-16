using System.Security.Claims;
using Application.Common.Adapters.Auth;
using Application.Contracts.Auth;
using Bogus;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;
using Infrastructure.Auth.Models;

namespace UnitTests.Factories;

internal static class TestDataFactory
{
    private static Faker<AppUser> UserFaker(bool useNewSeed = false, string? userName = null, string? email = null)
    {
        var seed = 10;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<AppUser>()
            .CustomInstantiator(faker =>
                AppUserFactory.Create(
                    userName ?? faker.Internet.UserName(),
                    email ?? faker.Internet.Email(),
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
    
    private static Faker<Token> TokenFaker(bool isExpired, bool useNewSeed = false)
    {
        var seed = 50;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<Token>()
            .CustomInstantiator(x =>
                {
                    var sid = x.Random.Guid().ToString("N");
                    var userId = x.Random.Guid();
                    var jtiHash = x.Random.Hash();
                    var refreshHash = x.Random.Hash();
                    var isLongSession = x.Random.Bool();

                    var token = Token.CreateToken(sid, userId, jtiHash, refreshHash, isLongSession);

                    if (isExpired)
                    {
                        typeof(Token)
                            .GetProperty("ExpiresAt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                            .SetValue(token, DateTime.UtcNow.AddMinutes(-1));
                    }

                    return token;
                }
            ).UseSeed(seed);
    }

    private static Faker<AccessTokenClaims> AccessTokenClaimsFaker(bool useNewSeed = false)
    {
        var seed = 60;
        if (useNewSeed)
        {
            seed = Random.Shared.Next(10, int.MaxValue);
        }
        
        return new Faker<AccessTokenClaims>()
            .CustomInstantiator(faker =>
                {
                    var uid = Guid.NewGuid().ToString();
                    var claims = new List<Claim>
                    {
                        new(TokenClaims.TokenId, Guid.NewGuid().ToString("N")),
                        new(TokenClaims.SessionId, Guid.NewGuid().ToString("N")),
                        new(TokenClaims.UserId, uid),
                        new(TokenClaims.Name, faker.Name.FirstName().ToLower()),
                        new(TokenClaims.Email, faker.Internet.Email()),
                        new(TokenClaims.Subject, uid),
                        new(TokenClaims.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        new(TokenClaims.Expiration, DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        new(TokenClaims.NotBefore, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        new(TokenClaims.Issuer, "Test issuer"),
                        new(TokenClaims.Audience, "Test audience"),
                        new(TokenClaims.Role, "User")
                    };
                    
                    var result = AccessTokenClaims.Create(claims);
                    if (!result.Succeeded || result.Data is null)
                        throw new Exception("Failed to create AccessTokenClaims for tests");

                    return result.Data;
                }
            ).UseSeed(seed);
    }


    // --- Factories ---
    // AppUser
    internal static AppUser CreateUser(bool useNewSeed = false, string? userName = null, string? email = null) => UserFaker(useNewSeed, userName, email).Generate();

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
    
    internal static Token CreateToken(bool isExpired = false, bool useNewSeed = false) => 
        TokenFaker(isExpired, useNewSeed).Generate();

    internal static AccessTokenClaims CreateAccessTokenClaims(bool useNewSeed = false) => 
        AccessTokenClaimsFaker(useNewSeed).Generate();

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