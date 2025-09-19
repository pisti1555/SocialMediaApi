using API.Controllers.Common;
using API.DTOs.Bodies.Posts.Comments;
using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;
using Application.Responses;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[Route("api/posts/{postId}/comments")]
public class PostCommentController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<PostCommentResponseDto>>> GetCommentsOfPost([FromRoute]string postId)
    {
        var command = new GetCommentsOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetCommentsOfPostQuery, List<PostCommentResponseDto>>(command);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostCommentResponseDto>> AddComment([FromRoute]string postId, [FromBody]AddCommentToPostDto dto)
    {
        var command = new AddCommentToPostCommand(postId, dto.UserId, dto.Text);
        var result = await mediator.SendCommandAsync<AddCommentToPostCommand, PostCommentResponseDto>(command);
        return Ok(result);
    }
    
    [HttpDelete("{commentId}")]
    public async Task<ActionResult> DeleteComment([FromRoute]string postId, [FromRoute]string commentId, [FromQuery]string userId)
    {
        var command = new RemoveCommentFromPostCommand(postId, commentId, userId);
        await mediator.SendCommandAsync<RemoveCommentFromPostCommand, Unit>(command);
        return Ok();
    }
}