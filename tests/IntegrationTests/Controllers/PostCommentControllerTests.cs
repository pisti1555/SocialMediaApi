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
using Xunit.Abstractions;

namespace IntegrationTests.Controllers;

public class PostCommentControllerTests(CustomWebApplicationFactoryFixture factory, ITestOutputHelper output) : BaseControllerTest(factory), IAsyncLifetime
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
    public async Task GetCommentsOfPost_WhenCommentsExist_ShouldCacheResult_ThenReturnFullList()
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
    public async Task AddComment_WenValidRequest_ShouldSaveCommentToDatabase_ThenReturnPost()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var dto = new AddCommentToPostDto("Test text");
        
        var response = await authenticatedClient.PostAsJsonAsync($"{PostsBaseUrl}/{_post.Id}/comments", dto);
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
    public async Task AddComment_WhenPostNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingPostId = Guid.NewGuid().ToString();
        var addCommentDto = new AddCommentToPostDto("Test text");
        
        var response = await authenticatedClient.PostAsJsonAsync($"{PostsBaseUrl}/{notExistingPostId}/comments", addCommentDto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateComment_WhenValidRequest_ShouldUpdateComment_ThenReturnComment()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var comment = await AddCommentToDbAsync(PostCommentDataFixture.GetPostComment(_user, _post));
        var dto = new UpdateCommentOfPostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{PostsBaseUrl}/{_post.Id}/comments/{comment.Id}", dto);
        var result = await response.Content.ReadFromJsonAsync<PostCommentResponseDto>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task UpdateComment_WhenPostNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingPostId = Guid.NewGuid().ToString();
        var commentId = PostCommentDataFixture.GetPostComment(_user, _post).Id.ToString();
        var updateCommentDto = new UpdateCommentOfPostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{PostsBaseUrl}/{notExistingPostId}/comments/{commentId}", updateCommentDto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateComment_WhenCommentNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingCommentId = Guid.NewGuid().ToString();
        var updateCommentDto = new UpdateCommentOfPostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/comments/{notExistingCommentId}", updateCommentDto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveComment_WhenValidRequest_ShouldReturnOk()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        await AddCommentToDbAsync(PostCommentDataFixture.GetPostComment(_user, _post));
        
        var comment = await DbContext.PostComments
            .Where(x => x.PostId == _post.Id)
            .FirstAsync();
        
        var response = await authenticatedClient.DeleteAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/comments/{comment.Id.ToString()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveComment_WhenPostNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingPostId = Guid.NewGuid().ToString();
        var notExistingCommentId = Guid.NewGuid().ToString();
        
        var response = await authenticatedClient.DeleteAsync($"{PostsBaseUrl}/{notExistingPostId}/comments/{notExistingCommentId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task RemoveComment_WhenCommentNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingCommentId = Guid.NewGuid().ToString();
        
        var response = await authenticatedClient.DeleteAsync($"{PostsBaseUrl}/{_post.Id.ToString()}/comments/{notExistingCommentId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
        
        await DbContext.Database.EnsureCreatedAsync();
        await IdentityDbContext.Database.EnsureCreatedAsync();
        
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
        await IdentityDbContext.Database.EnsureDeletedAsync();
    }
}