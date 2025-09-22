using System.Net;
using System.Net.Http.Json;
using API.DTOs.Bodies.Posts.Comments;
using Application.Responses;
using Domain.Posts;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Controllers;

public class PostCommentControllerTests(CustomWebApplicationFactoryFixture factory) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string PostsBaseUrl = "/api/v1/posts";
    private AppUser _user = null!;
    private Post _post = null!;
    
    private static string PostCommentsCacheKey(Guid postId) => $"post-comments-{postId.ToString()}";
    
    private async Task<PostComment> AddCommentToDbAsync(PostComment comment)
    {
        DbContext.PostComments.Add(comment);
        await DbContext.SaveChangesAsync();
        return comment;
    }
    
    private static void AssertCommentsMatch(List<PostComment> expected, List<PostCommentResponseDto>? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].User.Id, actual[i].UserId);
            Assert.Equal(expected[i].Post.Id, actual[i].PostId);
            Assert.Equal(expected[i].User.UserName, actual[i].UserName);
            Assert.Equal(expected[i].Text, actual[i].Text);
        }
    }

    [Fact]
    public async Task GetCommentsOfPost_ShouldCacheResult_ThenReturnFullList()
    {
        var comment = await AddCommentToDbAsync(PostCommentDataFixture.GetPostComment(_user, _post));
        
        var response = await Client.GetAsync($"{PostsBaseUrl}/{_post.Id}/comments");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        var cache = await Cache.GetAsync<List<PostCommentResponseDto>?>(PostCommentsCacheKey(_post.Id));
        Assert.NotNull(cache);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertCommentsMatch(comment.Post.Comments.ToList(), result);
        
        await Cache.RemoveAsync(PostCommentsCacheKey(_post.Id));
    }
    
    [Fact]
    public async Task GetCommentsOfPost_WhenNoComments_ShouldCacheResult_ThenReturnEmptyList()
    {
        var response = await Client.GetAsync($"{PostsBaseUrl}/{_post.Id}/comments");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        var cache = await Cache.GetAsync<List<PostCommentResponseDto>?>(PostCommentsCacheKey(_post.Id));
        Assert.NotNull(cache);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Empty(result);
        
        await Cache.RemoveAsync(PostCommentsCacheKey(_post.Id));
    }

    [Fact]
    public async Task AddComment_ShouldAddComment_ThenReturnPostCommentDto()
    {
        var dto = new AddCommentToPostDto(_user.Id.ToString(), "Test text");
        
        var response = await Client.PostAsJsonAsync($"{PostsBaseUrl}/{_post.Id}/comments", dto);
        var result = await response.Content.ReadFromJsonAsync<PostCommentResponseDto>();
        
        var commentInDb = await DbContext.PostComments
            .Where(x => x.PostId == _post.Id)
            .Include(x => x.User)
            .Include(x => x.Post)
            .FirstAsync();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(commentInDb.Id, result.Id);
        Assert.Equal(commentInDb.User.Id, result.UserId);
        Assert.Equal(commentInDb.Post.Id, result.PostId);
        Assert.Equal(commentInDb.Text, result.Text);
    }
    
    [Fact]
    public async Task AddComment_WhenPostNotFound_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsJsonAsync($"{PostsBaseUrl}/{Guid.NewGuid().ToString()}/comments", _user.Id.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveComment_ShouldReturnOk()
    {
        await AddCommentToDbAsync(PostCommentDataFixture.GetPostComment(_user, _post));
        
        var comment = await DbContext.PostComments
            .Where(x => x.PostId == _post.Id)
            .FirstAsync();
        
        var response = await Client.DeleteAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/comments/{comment.Id.ToString()}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveComment_WhenPostNotFound_ShouldReturnBadRequest()
    {
        var response = await Client.DeleteAsync($"{PostsBaseUrl}/{Guid.NewGuid().ToString()}/comments/{Guid.NewGuid().ToString()}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveComment_WhenCommentNotFound_ShouldReturnBadRequest()
    {
        var response = await Client.DeleteAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/comments/{Guid.NewGuid().ToString()}?userId={_user.Id.ToString()}");
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