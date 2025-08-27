using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Common.Pagination;
using Application.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Persistence.DataContext;

namespace Persistence.Repositories.Post;

public class PostRepository(
        AppDbContext context, 
        IPostLikeRepository likeRepository, 
        IPostCommentRepository commentRepository,
        IMapper mapper
    ) : IPostRepository
{
    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChangesAsync()
    {
        return context.ChangeTracker.HasChanges();
    }

    public void Add(Domain.Posts.Post post)
    {
        context.Posts.Add(post);
    }

    public void Update(Domain.Posts.Post post)
    {
        context.Posts.Update(post);
    }

    public void Delete(Domain.Posts.Post post)
    {
        context.Posts.Remove(post);
    }

    public async Task<Domain.Posts.Post?> GetByIdAsync(Guid id)
    {
        return await context.Posts
            .Include(x => x.User)
            .Include(x => x.Comments).OrderByDescending(x => x.CreatedAt)
            .Include(x => x.Likes)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Posts.AnyAsync(x => x.Id == id);
    }

    public async Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize)
    {
        var postsProjected = context.Posts
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<PostResponseDto>(mapper.ConfigurationProvider);
        
        var totalCount = await postsProjected.CountAsync();
        
        var pageResultOfPostsProjected = await postsProjected
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
            
        return PagedResult<PostResponseDto>.Create(pageResultOfPostsProjected, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<PostResponseDto>> GetDtoPagedAsync(int pageNumber, int pageSize, Guid userId)
    {
        var postsProjected = context.Posts
            .Where(x => x.UserId == userId)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<PostResponseDto>(mapper.ConfigurationProvider);
        
        var totalCount = await postsProjected.CountAsync();
        
        var pageResultOfPostsProjected = await postsProjected
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
            
        return PagedResult<PostResponseDto>.Create(pageResultOfPostsProjected, totalCount, pageNumber, pageSize);
    }

    public IPostLikeRepository LikeRepository => likeRepository;
    public IPostCommentRepository CommentRepository => commentRepository;
}