using Domain.Posts;

namespace Application.Common.Interfaces.Persistence.Repositories.Post;

public interface IPostLikeRepository
{
    void Add(PostLike like);
    void Delete(PostLike like);
    
    Task<PostLike?> GetByIdAsync(Guid id);
    Task<PostLike?> GetByUserIdAndPostIdAsync(Guid userId, Guid postId);
    
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(Guid postId, Guid userId);
    
    Task<IEnumerable<PostLike>> GetAllOfPostAsync(Guid postId);
    Task<int> CountOfPostAsync(Guid postId);
}