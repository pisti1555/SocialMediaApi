using Application.Common.Pagination;
using Application.Responses;
using Domain.Posts;

namespace Application.Common.Interfaces.Persistence.Repositories.Post;

public interface IPostCommentRepository
{
    void Add(PostComment comment);
    void Update(PostComment comment);
    void Delete(PostComment comment);
    
    Task<PostComment?> GetByIdAsync(Guid id);
    
    Task<bool> ExistsAsync(Guid postId, Guid userId);
    Task<bool> ExistsAsync(Guid id);
    
    Task<IEnumerable<PostComment>> GetAllCommentOfPostAsync(Guid postId);
    Task<int> CountOfPostAsync(Guid postId);
    
    // Public paged responses
    Task<PagedResult<PostCommentResponseDto>> GetPagedCommentDtoOfPostAsync(Guid postId, int pageNumber, int pageSize);
}