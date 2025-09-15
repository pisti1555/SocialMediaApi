using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Requests.Posts.Root.Queries.GetById;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Helpers;
using AutoMapper;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly IMapper _mapper = MapperHelper.GetMapper();
    private readonly GetPostByIdHandler _handler;

    private readonly Post _post;
    private readonly string _postCacheKey;

    public GetPostByIdHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetPostByIdHandler(_postRepositoryMock.Object, _cacheServiceMock.Object, _mapper);

        _post = TestDataFactory.CreatePostWithRelations().Post;
        _postCacheKey = $"post-{_post.Id.ToString()}";
    }

    [Fact]
    public async Task Handle_ShouldReturnPost_FromDatabase()
    {
        // Arrange
        var query = new GetPostByIdQuery(_post.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None))
            .ReturnsAsync((Post?)null);
        _postRepositoryMock
            .Setup(x => x.GetByIdAsync(_post.Id))
            .ReturnsAsync(_post);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        _cacheServiceMock.Verify(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, _post.Id);
        Assert.Equal(result.Text, _post.Text);
        Assert.Equal(result.CreatedAt, _post.CreatedAt);
        Assert.Equal(result.UserId, _post.User.Id);
        Assert.Equal(result.UserName, _post.User.UserName);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPost_FromCache()
    {
        // Arrange
        var query = new GetPostByIdQuery(_post.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None))
            .ReturnsAsync(_post);
        _postRepositoryMock
            .Setup(x => x.GetByIdAsync(_post.Id))
            .ReturnsAsync(_post);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        _cacheServiceMock.Verify(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Never);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, _post.Id);
        Assert.Equal(result.Text, _post.Text);
        Assert.Equal(result.CreatedAt, _post.CreatedAt);
        Assert.Equal(result.UserId, _post.User.Id);
        Assert.Equal(result.UserName, _post.User.UserName);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_PostDoesNotExist()
    {
        // Arrange
        var query = new GetPostByIdQuery(_post.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None))
            .ReturnsAsync((Post?)null);
        _postRepositoryMock
            .Setup(x => x.GetByIdAsync(_post.Id))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        
        _cacheServiceMock.Verify(x => x.GetAsync<Post>(_postCacheKey, CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_GUIDIsInvalid()
    {
        // Arrange
        var query = new GetPostByIdQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
        
        _cacheServiceMock.Verify(x => x.GetAsync<Post>(It.IsAny<string>(), CancellationToken.None), Times.Never);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}