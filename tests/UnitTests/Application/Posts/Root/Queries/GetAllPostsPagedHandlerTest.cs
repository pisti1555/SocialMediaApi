using Application.Common.Pagination;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Responses;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Queries;

public class GetAllPostsPagedHandlerTest : BasePostHandlerTest
{
    private readonly GetAllPostsPagedHandler _handler;
    
    private readonly PagedResult<PostResponseDto> _posts;

    public GetAllPostsPagedHandlerTest()
    {
        _handler = new GetAllPostsPagedHandler(PostRepositoryMock.Object);
        
        var postsList = TestDataFactory.CreatePosts(5);
        var postResponseDtoList = Mapper.Map<List<PostResponseDto>>(postsList);
        _posts = PagedResult<PostResponseDto>.Create(postResponseDtoList, postResponseDtoList.Count, 1, 10);
    }

    private static readonly GetAllPostsPagedQuery Query = new()
    {
        PageNumber = 1,
        PageSize = 10
    };
    
    private static void AssertPostsMatch(PagedResult<PostResponseDto> expected, PagedResult<PostResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(actual.Items);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.Items.Count, actual.Items.Count);
        
        for (var i = 0; i < expected.Items.Count; i++)
        {
            Assert.Equal(expected.Items[i].Id, actual.Items[i].Id);
            Assert.Equal(expected.Items[i].Text, actual.Items[i].Text);
            Assert.Equal(expected.Items[i].CreatedAt, actual.Items[i].CreatedAt);
            Assert.Equal(expected.Items[i].UserId, actual.Items[i].UserId);
            Assert.Equal(expected.Items[i].UserName, actual.Items[i].UserName);
            Assert.Equal(expected.Items[i].LikesCount, actual.Items[i].LikesCount);
            Assert.Equal(expected.Items[i].CommentsCount, actual.Items[i].CommentsCount);
        }
    }

    [Fact]
    public async Task Handle_WhenItems_ShouldReturnPagedResult()
    {
        // Arrange
        PostRepositoryMock.SetupGetPaged(_posts);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        PostRepositoryMock.VerifyGetPaged();
        
        AssertPostsMatch(_posts, result);
    }

    [Fact]
    public async Task Handle_WhenNoItemsFound_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var emptyPosts = PagedResult<PostResponseDto>.Create(new List<PostResponseDto>(), 0, 1, 10);
        
        PostRepositoryMock.SetupGetPaged(emptyPosts);
        
        var result = await _handler.Handle(Query, CancellationToken.None);
        
        // Assert
        PostRepositoryMock.VerifyGetPaged();
        
        AssertPostsMatch(emptyPosts, result);
    }
}