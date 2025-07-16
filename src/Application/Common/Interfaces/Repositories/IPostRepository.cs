using Application.Common.Pagination;
using Domain.Posts;
using Application.Responses;

namespace Application.Common.Interfaces.Repositories;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<PostResponseDto> GetPostDtoByIdAsync(Guid id);
    
    Task<PagedResult<PostResponseDto>> GetPostDtoPagedAsync(PaginationAttributes pagination);
    Task<PagedResult<PostResponseDto>> GetPostDtoPagedAsync(PaginationAttributes pagination, Guid userId);
    
    Task<List<PostCommentResponseDto>> GetAllCommentDtoOfPostAsync(Guid postId);
    Task<List<PostLikeResponseDto>> GetAllLikeDtoOfPostAsync(Guid postId);
}