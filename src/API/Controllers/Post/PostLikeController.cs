using API.Controllers.Common;
using Application.Requests.Posts.PostLike.Commands.DislikePost;
using Application.Requests.Posts.PostLike.Commands.LikePost;
using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using Application.Responses;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[Route("api/posts/{postId}/likes")]
public class PostLikeController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<PostLikeResponseDto>>> GetLikesOfPost([FromRoute]string postId)
    {
        var command = new GetLikesOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetLikesOfPostQuery, List<PostLikeResponseDto>>(command);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostLikeResponseDto>> LikePost([FromRoute]string postId, [FromQuery]string userId)
    {
        var command = new LikePostCommand(userId, postId);
        var result = await mediator.SendCommandAsync<LikePostCommand, PostLikeResponseDto>(command);
        return Ok(result);
    }
    
    [HttpDelete]
    public async Task<ActionResult> DislikePost([FromRoute]string postId, [FromQuery]string userId)
    {
        var command = new DislikePostCommand(postId, userId);
        await mediator.SendCommandAsync<DislikePostCommand, Unit>(command);
        return Ok();
    }
}