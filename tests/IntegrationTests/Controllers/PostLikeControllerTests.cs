using System.Net;
using System.Net.Http.Json;
using Application.Responses;
using Domain.Posts;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Controllers;

public class PostLikeControllerTests(CustomWebApplicationFactoryFixture factory) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string PostsBaseUrl = "/api/v1/posts";
    private AppUser _user = null!;
    private Post _post = null!;
    
    private static string PostLikesCacheKey(Guid postId) => $"post-likes-{postId.ToString()}";
    
    private async Task<PostLike> AddLikeToDbAsync(PostLike like)
    {
        DbContext.PostLikes.Add(like);
        await DbContext.SaveChangesAsync();
        return like;
    }
    
    private static void AssertLikesMatch(List<PostLike> expected, List<PostLikeResponseDto>? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].User.Id, actual[i].UserId);
            Assert.Equal(expected[i].Post.Id, actual[i].PostId);
            Assert.Equal(expected[i].User.UserName, actual[i].UserName);
        }
    }
    
    [Fact]
    public async Task GetLikesOfPost_ShouldCacheResult_ThenReturnFullList()
    {
        var like = await AddLikeToDbAsync(PostLikeDataFixture.GetPostLike(_user, _post));
        
        var response = await Client.GetAsync($"{PostsBaseUrl}/{_post.Id}/likes");
        var result = await response.Content.ReadFromJsonAsync<List<PostLikeResponseDto>>();
        
        var cache = await Cache.GetAsync<List<PostLikeResponseDto>?>(PostLikesCacheKey(_post.Id));
        Assert.NotNull(cache);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        AssertLikesMatch(like.Post.Likes.ToList(), result);
        
        await Cache.RemoveAsync(PostLikesCacheKey(_post.Id));
    }
    
    [Fact]
    public async Task GetLikesOfPost_WhenNoLikes_ShouldCacheResult_ThenReturnEmptyList()
    {
        var response = await Client.GetAsync($"{PostsBaseUrl}/{_post.Id}/likes");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        var cache = await Cache.GetAsync<List<PostLikeResponseDto>?>(PostLikesCacheKey(_post.Id));
        Assert.NotNull(cache);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(result);
        Assert.Empty(result);
        
        await Cache.RemoveAsync(PostLikesCacheKey(_post.Id));
    }

    [Fact]
    public async Task AddLike_ShouldAddLike_ThenReturnPostLikeDto()
    {
        var response = await Client.PostAsync($"{PostsBaseUrl}/{_post.Id}/likes?userId={_user.Id.ToString()}", null);
        var result = await response.Content.ReadFromJsonAsync<PostLikeResponseDto>();

        var likeInDb = await DbContext.PostLikes
            .Where(x => x.PostId == _post.Id)
            .Include(x => x.User)
            .Include(x => x.Post)
            .FirstAsync();
    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(result);
        Assert.Equal(likeInDb.Id, result.Id);
        Assert.Equal(likeInDb.User.Id, result.UserId);
        Assert.Equal(likeInDb.Post.Id, result.PostId);
        Assert.Equal(likeInDb.User.UserName, result.UserName);
    }
    
    [Fact]
    public async Task AddLike_WhenPostNotFound_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsJsonAsync($"{PostsBaseUrl}/{Guid.NewGuid().ToString()}/likes", _user.Id.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveLike_ShouldReturnOk()
    {
        var like = await AddLikeToDbAsync(PostLikeDataFixture.GetPostLike(_user, _post));
        var response = await Client.DeleteAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/likes/{like.Id.ToString()}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveLike_WhenPostNotFound_ShouldReturnBadRequest()
    {
        var unknownId = Guid.NewGuid().ToString();
        var response = await Client.DeleteAsync($"{PostsBaseUrl}/{Guid.NewGuid().ToString()}/likes/{unknownId}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
        
        _user = AppUserDataFixture.GetUser();
        _post = PostDataFixture.GetPost(_user);
        
        await DbContext.Users.AddAsync(_user);
        await DbContext.Posts.AddAsync(_post);
        await DbContext.SaveChangesAsync();
        
        _user = await DbContext.Users.FirstAsync();
        _post = await DbContext.Posts.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
    }
}