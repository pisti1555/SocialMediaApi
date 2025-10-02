using System.Net;
using System.Net.Http.Json;
using API.DTOs.Bodies.Posts.Root;
using Application.Common.Pagination;
using Application.Responses;
using Domain.Posts;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Controllers;

public class PostControllerTests(CustomWebApplicationFactoryFixture factory) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string BaseUrl = "/api/v1/posts";
    private AppUser _user = null!;
    
    private static string PostCacheKey(Guid postId) => $"post-{postId.ToString()}";
    private static string PostsPageCacheKey(int page, int size) => $"posts-{page}-{size}";

    private async Task<Post> AddPostToDbAsync(Post post)
    {
        DbContext.Posts.Add(post);
        await DbContext.SaveChangesAsync();
        return post;
    }

    private static void AssertPostsMatch(Post expected, PostResponseDto? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Text, actual.Text);
        Assert.Equal(expected.Likes.Count, actual.LikesCount);
        Assert.Equal(expected.Comments.Count, actual.CommentsCount);
        Assert.Equal(expected.User.Id, actual.UserId);
        Assert.Equal(expected.User.UserName, actual.UserName);
    }

    private static void AssertPaginationHeadersAreValid<T>(HttpResponseMessage response, PagedResult<T>? result)
    {
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        var currentPage = response.Headers.GetValues("X-Current-Page").First();
        var pageSize = response.Headers.GetValues("X-Page-Size").First();
        var totalItems = response.Headers.GetValues("X-Total-Items").First();
        var totalPages = response.Headers.GetValues("X-Total-Pages").First();
        
        Assert.NotNull(result);
        Assert.Equal(result.PageNumber, int.Parse(currentPage));
        Assert.Equal(result.PageSize, int.Parse(pageSize));
        Assert.Equal(result.TotalCount, int.Parse(totalItems));
        Assert.Equal(result.TotalPages, int.Parse(totalPages));
    }
    
    [Fact]
    public async Task Create_WhenValidRequest_ShouldReturnCreatedResponse_WithLocationHeader_AndPostDto()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var dto = new CreatePostDto("Test text");
        
        var response = await authenticatedClient.PostAsJsonAsync($"{BaseUrl}", dto);
        
        var result = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        
        var postInDb = await DbContext.Posts
            .Include(x => x.User)
            .Include(x => x.Comments)
            .Include(x => x.Likes)
            .FirstAsync();
        
        var locationHeaderContent = response.Headers.Location?.ToString();
        Assert.Equal(postInDb.Id.ToString(), locationHeaderContent?.Split('/').Last());
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));
        
        AssertPostsMatch(postInDb, result);
        
        await Cache.RemoveAsync(PostCacheKey(postInDb.Id));
    }
    
    [Fact]
    public async Task Create_WhenUnauthenticated_ShouldReturnUnauthorizedResponse()
    {
        var dto = new CreatePostDto("Test text");
        
        var response = await Client.PostAsJsonAsync(BaseUrl, dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_WhenValidRequest_ShouldReturnUpdatedPost()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var post = await AddPostToDbAsync(PostDataFixture.GetPost(_user));
        
        var dto = new UpdatePostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{BaseUrl}/{post.Id.ToString()}", dto);
        var result = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Updated text", result?.Text);
        
        await Cache.RemoveAsync(PostCacheKey(post.Id));
    }
    
    [Fact]
    public async Task Update_WhenPostNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingPostId = Guid.NewGuid().ToString();
        var dto = new UpdatePostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{BaseUrl}/{notExistingPostId}", dto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_WhenUserDoesNotOwnPost_ShouldReturnBadRequest()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        
        var otherUser = AppUserDataFixture.GetUser();
        var post = await AddPostToDbAsync(PostDataFixture.GetPost(otherUser));
        
        var dto = new UpdatePostDto("Updated text");
        
        var response = await authenticatedClient.PatchAsJsonAsync($"{BaseUrl}/{post.Id.ToString()}", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenValidRequest_ShouldReturnOk()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var post = await AddPostToDbAsync(PostDataFixture.GetPost(_user));
        
        var response = await authenticatedClient.DeleteAsync($"{BaseUrl}/{post.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_WhenPostNotFound_ShouldReturnNotFound()
    {
        var authenticatedClient = await GetAuthenticatedClientAsync(_user);
        var notExistingPostId = Guid.NewGuid().ToString();
        
        var response = await authenticatedClient.DeleteAsync($"{BaseUrl}/{notExistingPostId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetById_WhenExistsById_ShouldSaveToCache_ThenReturnPost()
    {
        var post = await AddPostToDbAsync(PostDataFixture.GetPost(_user));
        
        var response = await Client.GetAsync($"{BaseUrl}/{post.Id}");
        var result = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        
        var cachedPost = await Cache.GetAsync<PostResponseDto>(PostCacheKey(post.Id));
        Assert.NotNull(cachedPost);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        AssertPostsMatch(post, result);
        
        await Cache.RemoveAsync(PostCacheKey(post.Id));
    }
    
    [Fact]
    public async Task GetById_WhenPostNotFound_ShouldReturnNotFound()
    {
        var notExistingPostId = Guid.NewGuid().ToString();
        
        var response = await Client.GetAsync($"{BaseUrl}/{notExistingPostId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllPaged_WhenNoPostsExist_ShouldReturnEmptyList()
    {
        var response = await Client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PostResponseDto>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        AssertPaginationHeadersAreValid(response, result);
        
        await Cache.RemoveAsync(PostsPageCacheKey(1, 10));
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
        
        await DbContext.Database.EnsureCreatedAsync();
        await IdentityDbContext.Database.EnsureCreatedAsync();
        
        _user = AppUserDataFixture.GetUser();
        
        await DbContext.Users.AddAsync(_user);
        await DbContext.SaveChangesAsync();
        
        _user = await DbContext.Users.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
    }
}