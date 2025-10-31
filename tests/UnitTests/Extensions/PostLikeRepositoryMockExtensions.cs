using System.Linq.Expressions;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using AutoMapper;
using Moq;
using XLike = Domain.Posts.PostLike;

namespace UnitTests.Extensions;

public static class PostLikeRepositoryMockExtensions
{
    public static void SetupLike(this Mock<IRepository<XLike>> likeRepositoryMock, XLike? like)
    {
        likeRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => like != null && id == like.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(like);
    }
    
    public static void SetupLike(this Mock<IRepository<XLike, PostLikeResponseDto>> likeRepositoryMock, XLike? like, IMapper mapper)
    {
        likeRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => like != null && id == like.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(like is not null ? mapper.Map<PostLikeResponseDto>(like) : null);
    }
    
    public static void SetupLikes(this Mock<IRepository<XLike, PostLikeResponseDto>> likeRepositoryMock, List<PostLikeResponseDto> likes)
    {
        likeRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<XLike, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(likes);
    }
    
    public static void SetupLikeExists(this Mock<IRepository<XLike>> likeRepositoryMock, Guid likeId, bool exists)
    {
        likeRepositoryMock
            .Setup(x => x.ExistsAsync(It.Is<Guid>(id => id == likeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupLikeExists(this Mock<IRepository<XLike, PostLikeResponseDto>> likeRepositoryMock, Guid likeId, bool exists)
    {
        likeRepositoryMock
            .Setup(x => x.ExistsAsync(It.Is<Guid>(id => id == likeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupLikeExists(this Mock<IRepository<XLike>> likeRepositoryMock, bool exists)
    {
        likeRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<XLike, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupLikeExists(this Mock<IRepository<XLike, PostLikeResponseDto>> likeRepositoryMock, bool exists)
    {
        likeRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<XLike, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
}