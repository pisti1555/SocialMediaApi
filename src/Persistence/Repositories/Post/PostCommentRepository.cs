using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Common.Pagination;
using Application.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Persistence.DataContext;

namespace Persistence.Repositories.Post;

public class PostCommentRepository(AppDbContext context, IMapper mapper) : IPostCommentRepository
{
    public void Add(PostComment comment)
    {
        context.PostComments.Add(comment);
    }

    public void Update(PostComment comment)
    {
        context.PostComments.Update(comment);
    }

    public void Delete(PostComment comment)
    {
        context.PostComments.Remove(comment);
    }

    public async Task<PostComment?> GetByIdAsync(Guid id)
    {
        return await context.PostComments
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid postId, Guid userId)
    {
        return await context.PostComments.AnyAsync(x => x.PostId == postId && x.UserId == userId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.PostComments.AnyAsync(x => x.Id == id);   
    }

    public async Task<IEnumerable<PostComment>> GetAllCommentOfPostAsync(Guid postId)
    {
        return await context.PostComments
            .Where(x => x.PostId == postId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountOfPostAsync(Guid postId)
    {
        return await context.PostComments.CountAsync(x => x.PostId == postId);   
    }

    public async Task<PagedResult<PostCommentResponseDto>> GetPagedCommentDtoOfPostAsync(Guid postId, int pageNumber, int pageSize)
    {
        var commentsProjected = context.PostComments
            .Where(x => x.PostId == postId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<PostCommentResponseDto>(mapper.ConfigurationProvider);
        
        var totalCount = await commentsProjected.CountAsync();
        
        var pageResultOfCommentsProjected = await commentsProjected
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
            
        return PagedResult<PostCommentResponseDto>.Create(pageResultOfCommentsProjected, totalCount, pageNumber, pageSize);
    }
}