using Application.Common.Interfaces.Repositories;
using Application.Common.Pagination;
using Application.Responses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Posts;
using Infrastructure.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PostRepository(AppDbContext context, IMapper mapper) : BaseRepository<Post>(context), IPostRepository
{
    public async Task<PostResponseDto> GetPostDtoByIdAsync(Guid id)
    {
        var result = await context.Posts.FindAsync(id);
        return mapper.Map<PostResponseDto>(result);
    }

    public async Task<PagedResult<PostResponseDto>> GetPostDtoPagedAsync(PaginationAttributes pagination)
    {
        var totalCount = context.Posts.Count();
        var result = await context.Posts
            .ProjectTo<PostResponseDto>(mapper.ConfigurationProvider)
            .Skip(pagination.PageSize * (pagination.PageNumber - 1))
            .Take(pagination.PageSize)
            .ToListAsync();
        
        return PagedResult<PostResponseDto>.Create(
            result, totalCount, pagination.PageNumber, pagination.PageSize
        );
    }

    public async Task<PagedResult<PostResponseDto>> GetPostDtoPagedAsync(PaginationAttributes pagination, Guid userId)
    {
        var totalCount = context.Posts.Count();
        var result = await context.Posts
            .Include(x => x.Comments)
            .Include(x => x.Likes)
            .Include(x => x.User)
            .ProjectTo<PostResponseDto>(mapper.ConfigurationProvider)
            .Where(x => x.UserId == userId)
            .Skip(pagination.PageSize * (pagination.PageNumber - 1))
            .Take(pagination.PageSize)
            .ToListAsync();
        
        return PagedResult<PostResponseDto>.Create(
            result, totalCount, pagination.PageNumber, pagination.PageSize
        );
    }

    public async Task<List<PostCommentResponseDto>> GetAllCommentDtoOfPostAsync(Guid postId)
    {
        return await context.PostComments
            .ProjectTo<PostCommentResponseDto>(mapper.ConfigurationProvider)
            .Where(x => x.PostId == postId)
            .ToListAsync();
    }

    public async Task<List<PostLikeResponseDto>> GetAllLikeDtoOfPostAsync(Guid postId)
    {
        return await context.PostLikes
            .ProjectTo<PostLikeResponseDto>(mapper.ConfigurationProvider)
            .Where(x => x.PostId == postId)
            .ToListAsync();
    }
}