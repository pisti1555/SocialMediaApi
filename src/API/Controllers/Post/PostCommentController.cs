using API.Controllers.Common;
using API.DTOs.Bodies.Posts.Comments;
using API.Extensions;
using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;
using Application.Requests.Posts.PostComment.Queries.GetAllOfPost;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/posts/{postId}/comments")]
public class PostCommentController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<List<PostCommentResponseDto>>> GetCommentsOfPost([FromRoute]string postId, CancellationToken ct)
    {
        var command = new GetCommentsOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetCommentsOfPostQuery, List<PostCommentResponseDto>>(command, ct);
        return Ok(result);
    }
    
    [Authorize]
    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<PostCommentResponseDto>> AddComment([FromRoute]string postId, [FromBody]AddCommentToPostDto dto, CancellationToken ct)
    {
        var command = new AddCommentToPostCommand(
            postId, 
            User.GetUserId(),
            dto.Text ?? string.Empty
        );
        
        var result = await mediator.SendCommandAsync<AddCommentToPostCommand, PostCommentResponseDto>(command, ct);
        return Ok(result);
    }
    
    [Authorize]
    [MapToApiVersion(1)]
    [HttpPatch("{commentId}")]
    public async Task<ActionResult<PostCommentResponseDto>> UpdateComment([FromRoute]string postId, [FromRoute]string commentId, [FromBody]UpdateCommentOfPostDto dto, CancellationToken ct)
    {
        var command = new UpdateCommentOfPostCommand(
            postId, 
            commentId, 
            User.GetUserId(),
            dto.Text ?? string.Empty
        );
        
        var result = await mediator.SendCommandAsync<UpdateCommentOfPostCommand, PostCommentResponseDto>(command, ct);
        return Ok(result);
    }
    
    [Authorize]
    [MapToApiVersion(1)]
    [HttpDelete("{commentId}")]
    public async Task<ActionResult> DeleteComment([FromRoute]string postId, [FromRoute]string commentId, CancellationToken ct)
    {
        var command = new RemoveCommentFromPostCommand(
            postId, 
            commentId, 
            User.GetUserId()
        );
        
        await mediator.SendCommandAsync<RemoveCommentFromPostCommand, Unit>(command, ct);
        return Ok();
    }
}