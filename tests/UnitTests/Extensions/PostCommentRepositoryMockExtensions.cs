using System.Linq.Expressions;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using AutoMapper;
using Moq;
using XComment = Domain.Posts.PostComment;

namespace UnitTests.Extensions;

public static class PostCommentRepositoryMockExtensions
{
    public static void SetupComment(this Mock<IRepository<XComment, PostCommentResponseDto>> commentRepositoryMock, XComment? comment, IMapper mapper)
    {
        commentRepositoryMock
            .Setup(x => x.GetEntityByIdAsync(It.Is<Guid>(id => comment != null && id == comment.Id)))
            .ReturnsAsync(comment);
        
        commentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => comment != null && id == comment.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment is not null ? mapper.Map<PostCommentResponseDto>(comment) : null);
    }
    
    public static void SetupComments(this Mock<IRepository<XComment, PostCommentResponseDto>> commentRepositoryMock, List<PostCommentResponseDto> comments)
    {
        commentRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<XComment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);
    }
}