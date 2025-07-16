using API.DTOs.Params;
using API.Extensions;
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
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PostController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> Create([FromBody]CreatePostCommand command)
    {
        return Ok(await mediator.Send(command));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]string id)
    {
        var command = new DeletePostCommand(id);
        return Ok(await mediator.Send(command));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetById([FromRoute]string id)
    {
        var query = new GetPostByIdQuery(id);
        return Ok(await mediator.Send(query));
    }
    
    [HttpGet]
    public async Task<ActionResult<PostResponseDto>> GetAllPaged([FromQuery]PaginationParams pagination)
    {
        var query = new GetAllPostsPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.Send(query);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
    
    // Comments
    [HttpGet("{postId}/comments")]
    public async Task<ActionResult<List<PostCommentResponseDto>>> GetCommentsOfPost(string postId)
    {
        var command = new GetCommentsOfPostQuery(postId);
        return Ok(await mediator.Send(command));
    }
    
    [HttpPost("{postId}/comments")]
    public async Task<ActionResult> AddComment([FromRoute]string postId, [FromBody]string text)
    {
        var userId = "TODO";
        var command = new AddCommentToPostCommand(postId, userId, text);
        await mediator.Send(command);
        return Ok();
    }
    
    [HttpDelete("{postId}/comments/{commentId}")]
    public async Task<ActionResult> DeleteComment([FromRoute]string postId, [FromRoute]string commentId)
    {
        var userId = "TODO";
        var command = new RemoveCommentFromPostCommand(postId, commentId, userId);
        await mediator.Send(command);
        return Ok();
    }
    
    // Likes
    [HttpGet("{postId}/likes")]
    public async Task<ActionResult<List<PostLikeResponseDto>>> GetLikesOfPost(string postId)
    {
        var command = new GetLikesOfPostQuery(postId);
        return Ok(await mediator.Send(command));
    }
    
    [HttpPost("{postId}/likes")]
    public async Task<ActionResult> LikePost([FromRoute]string postId, [FromBody]string userId)
    {
        var command = new LikePostCommand(userId, postId);
        await mediator.Send(command);
        return Ok();
    }
    
    [HttpDelete("{postId}/likes")]
    public async Task<ActionResult> DislikePost([FromRoute]string postId)
    {
        var userId = "TODO";
        var command = new DislikePostCommand(postId, userId);
        await mediator.Send(command);
        return Ok();
    }
}