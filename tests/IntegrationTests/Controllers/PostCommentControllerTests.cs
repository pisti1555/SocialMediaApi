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
    private AppUser _user = null!;
    private Post _post = null!;

    [Fact]
    public async Task GetCommentsOfPost_ShouldReturnOkList()
    {
        var comment = PostCommentDataFixture.GetPostComment(_user, _post);
        DbContext.PostComments.Add(comment);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.GetAsync($"/api/post/{_post.Id}/comments");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(comment.Id, result[0].Id);
        Assert.Equal(comment.UserId, result[0].UserId);
        Assert.Equal(comment.PostId, result[0].PostId);
    }
    
    [Fact]
    public async Task GetCommentsOfPost_ShouldReturnEmptyList()
    {
        var response = await Client.GetAsync($"/api/post/{_post.Id}/comments");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Add_ShouldReturnOkResponse_WithPostCommentDto()
    {
        var dto = new AddCommentToPostDto(_user.Id.ToString(), "Test text");
        
        var response = await Client.PostAsJsonAsync($"/api/post/{_post.Id}/comments", dto);
        var result = await response.Content.ReadFromJsonAsync<PostCommentResponseDto>();
        
        var firstFoundCommentOfPost = await DbContext.PostComments
            .Where(x => x.PostId == _post.Id)
            .FirstAsync();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(firstFoundCommentOfPost.Id, result.Id);
        Assert.Equal(firstFoundCommentOfPost.UserId, result.UserId);
        Assert.Equal(firstFoundCommentOfPost.PostId, result.PostId);
        Assert.Equal(firstFoundCommentOfPost.Text, result.Text);
    }
    
    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenPostNotFound()
    {
        var response = await Client.PostAsJsonAsync($"/api/post/{Guid.NewGuid().ToString()}/comments", _user.Id.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnOk()
    {
        var comment = PostCommentDataFixture.GetPostComment(_user, _post);
        DbContext.PostComments.Add(comment);
        await DbContext.SaveChangesAsync();
        
        var firstFoundCommentOfPost = await DbContext.PostComments
            .Where(x => x.PostId == _post.Id)
            .FirstAsync();
        
        var response = await Client.DeleteAsync($"/api/post/{_post.Id.ToString()}/comments/{firstFoundCommentOfPost.Id.ToString()}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenPostNotFound()
    {
        var response = await Client.DeleteAsync($"/api/post/{Guid.NewGuid().ToString()}/comments/{Guid.NewGuid().ToString()}?userId={_user.Id.ToString()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenCommentNotFound()
    {
        var response = await Client.DeleteAsync($"/api/post/{_post.Id.ToString()}/comments/{Guid.NewGuid().ToString()}?userId={_user.Id.ToString()}");
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