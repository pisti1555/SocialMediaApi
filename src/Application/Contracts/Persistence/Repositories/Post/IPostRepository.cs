using Application.Common.Pagination;
using Application.Responses;
using XPost = Domain.Posts.Post;

namespace Application.Contracts.Persistence.Repositories.Post;

public interface IPostRepository : IRepositoryBase
{
    void Add(XPost post);
    void Update(XPost post);
    void Delete(XPost post);
    
    Task<XPost?> GetByIdAsync(Guid id);
    
    Task<bool> ExistsAsync(Guid id);
    
    // Public paged responses
    Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize);
    Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize, Guid userId);
    
    // Children
    IPostLikeRepository LikeRepository { get; }
    IPostCommentRepository CommentRepository { get; }
}