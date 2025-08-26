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
    private AppUser _user = null!;
    private Post _post = null!;
    
    [Fact]
    public async Task GetLikesOfPost_ShouldReturnOkList()
    {
        var like = PostLikeDataFixture.GetPostLike(_user, _post);
        DbContext.PostLikes.Add(like);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.GetAsync($"/api/post/{_post.Id}/likes");
        var result = await response.Content.ReadFromJsonAsync<List<PostLikeResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(like.Id, result[0].Id);
        Assert.Equal(like.UserId, result[0].UserId);
        Assert.Equal(like.PostId, result[0].PostId);
    }
    
    [Fact]
    public async Task GetLikesOfPost_ShouldReturnEmptyList()
    {
        var response = await Client.GetAsync($"/api/post/{_post.Id}/likes");
        var result = await response.Content.ReadFromJsonAsync<List<PostCommentResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Like_ShouldReturnOkResponse_WithPostLikeDto()
    {
        var response = await Client.PostAsync($"/api/post/{_post.Id}/likes?userId={_user.Id.ToString()}", null);
        var result = await response.Content.ReadFromJsonAsync<PostLikeResponseDto>();

        var firstFoundLikeOfPost = await DbContext.PostLikes
            .Where(x => x.PostId == _post.Id)
            .FirstAsync();
    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(firstFoundLikeOfPost.Id, result.Id);
        Assert.Equal(firstFoundLikeOfPost.UserId, result.UserId);
        Assert.Equal(firstFoundLikeOfPost.PostId, result.PostId);
    }
    
    [Fact]
    public async Task Like_ShouldReturnBadRequest_WhenPostNotFound()
    {
        var response = await Client.PostAsJsonAsync($"/api/post/{Guid.NewGuid().ToString()}/likes", _user.Id.ToString());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Dislike_ShouldReturnOk()
    {
        var like = PostLikeDataFixture.GetPostLike(_user, _post);
        DbContext.PostLikes.Add(like);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.DeleteAsync($"/api/post/{_post.Id.ToString()}/likes?userId={_user.Id.ToString()}");
 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Dislike_ShouldReturnBadRequest_WhenPostNotFound()
    {
        var response = await Client.DeleteAsync($"/api/post/{Guid.NewGuid().ToString()}/likes?userId={_user.Id.ToString()}");
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