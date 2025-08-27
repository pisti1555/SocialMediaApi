using Application.Common.Mappings;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using ApplicationUnitTests.Common;
using AutoMapper;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Queries;

public class GetLikesOfPostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostLikeRepository> _likeRepositoryMock;
    private readonly GetLikesOfPostHandler _handler;

    public GetLikesOfPostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _likeRepositoryMock = new Mock<IPostLikeRepository>();
        
        _postRepositoryMock.SetupGet(x => x.LikeRepository).Returns(_likeRepositoryMock.Object);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<PostProfile>());
        mapperConfig.AssertConfigurationIsValid();

        var mapper = mapperConfig.CreateMapper();

        _handler = new GetLikesOfPostHandler(_postRepositoryMock.Object, mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkList()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = PostLikeFactory.Create(user, post);
        var likes = new List<Domain.Posts.PostLike> { like };
        
        var query = new GetLikesOfPostQuery(post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.ExistsAsync(post.Id)).ReturnsAsync(true);
        _likeRepositoryMock.Setup(x => x.GetAllOfPostAsync(post.Id)).ReturnsAsync(likes);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        _postRepositoryMock.Verify(x => x.ExistsAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.GetAllOfPostAsync(post.Id), Times.Once);
        Assert.Single(result);
        Assert.Equal(like.Id, result[0].Id);
        Assert.Equal(like.UserId, result[0].UserId);
        Assert.Equal(like.PostId, result[0].PostId);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        
        var query = new GetLikesOfPostQuery(post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.ExistsAsync(post.Id)).ReturnsAsync(true);
        _likeRepositoryMock.Setup(x => x.GetAllOfPostAsync(post.Id)).ReturnsAsync(new List<Domain.Posts.PostLike>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        _postRepositoryMock.Verify(x => x.ExistsAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.GetAllOfPostAsync(post.Id), Times.Once);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_PostNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = PostLikeFactory.Create(user, post);
        var likes = new List<Domain.Posts.PostLike> { like };
        
        var query = new GetLikesOfPostQuery(post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.ExistsAsync(post.Id)).ReturnsAsync(false);
        _likeRepositoryMock.Setup(x => x.GetAllOfPostAsync(post.Id)).ReturnsAsync(likes);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

        _postRepositoryMock.Verify(x => x.ExistsAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.GetAllOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}