using API.DTOs.Bodies.Posts.Comments;
using API.DTOs.Bodies.Posts.Root;
using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;
using Application.Requests.Posts.PostLike.Commands.DislikePost;
using Application.Requests.Posts.PostLike.Commands.LikePost;
using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using Application.Requests.Posts.Root.Commands.CreatePost;
using Application.Requests.Posts.Root.Commands.DeletePost;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Requests.Posts.Root.Queries.GetById;
using Application.Responses;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PostController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> Create([FromBody]CreatePostDto dto)
    {
        var command = new CreatePostCommand(dto.Text, dto.UserId);
        var result = await mediator.SendCommandAsync<CreatePostCommand, PostResponseDto>(command);
        return CreatedAtAction(nameof(GetById), new {id = result.Id}, result);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]string id)
    {
        var command = new DeletePostCommand(id);
        await mediator.SendCommandAsync<DeletePostCommand, Unit>(command);
        return Ok();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetById([FromRoute]string id)
    {
        var query = new GetPostByIdQuery(id);
        var result = await mediator.SendQueryAsync<GetPostByIdQuery, PostResponseDto>(query);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostResponseDto>>> GetAllPaged([FromQuery]PaginationParams pagination)
    {
        var query = new GetAllPostsPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.SendQueryAsync<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>(query);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
    
    // Comments
    [HttpGet("{postId}/comments")]
    public async Task<ActionResult<List<PostCommentResponseDto>>> GetCommentsOfPost([FromRoute]string postId)
    {
        var command = new GetCommentsOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetCommentsOfPostQuery, List<PostCommentResponseDto>>(command);
        return Ok(result);
    }
    
    [HttpPost("{postId}/comments")]
    public async Task<ActionResult<PostCommentResponseDto>> AddComment([FromRoute]string postId, [FromBody]AddCommentToPostDto dto)
    {
        var command = new AddCommentToPostCommand(postId, dto.UserId, dto.Text);
        var result = await mediator.SendCommandAsync<AddCommentToPostCommand, PostCommentResponseDto>(command);
        return Ok(result);
    }
    
    [HttpDelete("{postId}/comments/{commentId}")]
    public async Task<ActionResult> DeleteComment([FromRoute]string postId, [FromRoute]string commentId, [FromQuery]string userId)
    {
        var command = new RemoveCommentFromPostCommand(postId, commentId, userId);
        await mediator.SendCommandAsync<RemoveCommentFromPostCommand, Unit>(command);
        return Ok();
    }
    
    // Likes
    [HttpGet("{postId}/likes")]
    public async Task<ActionResult<List<PostLikeResponseDto>>> GetLikesOfPost([FromRoute]string postId)
    {
        var command = new GetLikesOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetLikesOfPostQuery, List<PostLikeResponseDto>>(command);
        return Ok(result);
    }
    
    [HttpPost("{postId}/likes")]
    public async Task<ActionResult<PostLikeResponseDto>> LikePost([FromRoute]string postId, [FromQuery]string userId)
    {
        var command = new LikePostCommand(userId, postId);
        var result = await mediator.SendCommandAsync<LikePostCommand, PostLikeResponseDto>(command);
        return Ok(result);
    }
    
    [HttpDelete("{postId}/likes")]
    public async Task<ActionResult> DislikePost([FromRoute]string postId, [FromQuery]string userId)
    {
        var command = new DislikePostCommand(postId, userId);
        await mediator.SendCommandAsync<DislikePostCommand, Unit>(command);
        return Ok();
    }
}