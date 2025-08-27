using Application.Contracts.Persistence.Repositories.Post;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Persistence.DataContext;

namespace Persistence.Repositories.Post;

public class PostLikeRepository(AppDbContext context) : IPostLikeRepository
{
    public void Add(PostLike like)
    {
        context.PostLikes.Add(like);
    }

    public void Delete(PostLike like)
    {
        context.PostLikes.Remove(like);
    }

    public async Task<PostLike?> GetByIdAsync(Guid id)
    {
        return await context.PostLikes.FindAsync(id);
    }
    
    public async Task<PostLike?> GetByUserIdAndPostIdAsync(Guid userId, Guid postId)
    {
        return await context.PostLikes.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.PostLikes.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid postId, Guid userId)
    {
        return await context.PostLikes.AnyAsync(x => x.PostId == postId && x.UserId == userId);
    }

    public async Task<IEnumerable<PostLike>> GetAllOfPostAsync(Guid postId)
    {
        return await context.PostLikes.Where(x => x.PostId == postId)
            .OrderByDescending(x => x.CreatedAt)
            .Include(x => x.User)
            .ToListAsync();
    }

    public async Task<int> CountOfPostAsync(Guid postId)
    {
        return await context.PostLikes.CountAsync(x => x.PostId == postId);
    }
}