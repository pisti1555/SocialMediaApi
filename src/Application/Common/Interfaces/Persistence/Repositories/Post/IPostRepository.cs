using Application.Common.Pagination;
using Application.Responses;

namespace Application.Common.Interfaces.Persistence.Repositories.Post;

public interface IPostRepository : IRepositoryBase
{
    void Add(Domain.Posts.Post post);
    void Update(Domain.Posts.Post post);
    void Delete(Domain.Posts.Post post);
    
    Task<Domain.Posts.Post?> GetByIdAsync(Guid id);
    
    Task<bool> ExistsAsync(Guid id);
    
    // Public paged responses
    Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize);
    Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize, Guid userId);
    
    // Children
    IPostLikeRepository LikeRepository { get; }
    IPostCommentRepository CommentRepository { get; }
}