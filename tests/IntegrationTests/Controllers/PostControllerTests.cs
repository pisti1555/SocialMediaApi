using System.Net;
using System.Net.Http.Json;
using API.DTOs.Bodies.Posts.Root;
using Application.Common.Pagination;
using Application.Responses;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Controllers;

public class PostControllerTests(CustomWebApplicationFactoryFixture factory) : BaseControllerTest(factory), IAsyncLifetime
{
    private AppUser _user = null!;
    
    [Fact]
    public async Task Create_ShouldReturnCreatedResponse_WithLocationHeader_AndPostDto()
    {
        var dto = new CreatePostDto("Test text", _user.Id.ToString());
        
        var response = await Client.PostAsJsonAsync("/api/post", dto);
        var result = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));
        
        var firstFoundPostInDb = await DbContext.Posts
            .Include(post => post.User)
            .Include(post => post.Comments)
            .Include(post => post.Likes)
            .FirstAsync();
        
        var locationHeaderContent = response.Headers.Location?.ToString();
        
        Assert.Equal(firstFoundPostInDb.Id.ToString(), locationHeaderContent?.Split('/').Last());
        Assert.NotNull(result);
        
        Assert.Equal(firstFoundPostInDb.Id, result.Id);
        Assert.Equal(firstFoundPostInDb.Text, result.Text);
        Assert.Equal(firstFoundPostInDb.User.Id, result.UserId);
        Assert.Equal(firstFoundPostInDb.User.UserName, result.UserName);
        Assert.Equal(firstFoundPostInDb.Comments.Count, result.CommentsCount);
        Assert.Equal(firstFoundPostInDb.Likes.Count, result.LikesCount);
    }
    
    [Fact]
    public async Task Create_ShouldReturnBadRequestResponse_WhenUserNotFound()
    {
        var dto = new CreatePostDto("Test text", Guid.NewGuid().ToString());
        var response = await Client.PostAsJsonAsync("/api/post", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnOkResponse()
    {
        var post = PostDataFixture.GetPost(_user);
        DbContext.Posts.Add(post);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.DeleteAsync($"/api/post/{post.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnBadRequestResponse_WhenPostNotFound()
    {
        var response = await Client.DeleteAsync($"/api/post/{Guid.NewGuid().ToString()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnPostResponseDto()
    {
        var post = PostDataFixture.GetPost(_user);
        DbContext.Posts.Add(post);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.GetAsync($"/api/post/{post.Id}");
        var result = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        
        Assert.Equal(post.Id, result.Id);
        Assert.Equal(post.Text, result.Text);
        Assert.Equal(post.User.Id, result.UserId);
        Assert.Equal(post.User.UserName, result.UserName);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnNotFoundResponse()
    {
        var response = await Client.GetAsync($"/api/post/{Guid.NewGuid().ToString()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllPaged_ShouldReturnListWith1Post()
    {
        var post = PostDataFixture.GetPost(_user);
        DbContext.Posts.Add(post);
        await DbContext.SaveChangesAsync();
        
        var response = await Client.GetAsync("/api/post?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PostResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetAllPaged_ShouldReturnEmptyList()
    {
        var response = await Client.GetAsync("/api/post?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PostResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.PageNumber);
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
        
        _user = AppUserDataFixture.GetUser();
        
        await DbContext.Users.AddAsync(_user);
        await DbContext.SaveChangesAsync();
        
        _user = await DbContext.Users.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
    }
}