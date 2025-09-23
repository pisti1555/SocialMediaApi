using API.Controllers.Common;
using Application.Requests.Posts.PostLike.Commands.DislikePost;
using Application.Requests.Posts.PostLike.Commands.LikePost;
using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/posts/{postId}/likes")]
public class PostLikeController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<List<PostLikeResponseDto>>> GetLikesOfPost([FromRoute]string postId)
    {
        var command = new GetLikesOfPostQuery(postId);
        var result = await mediator.SendQueryAsync<GetLikesOfPostQuery, List<PostLikeResponseDto>>(command);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<PostLikeResponseDto>> LikePost([FromRoute]string postId, [FromQuery]string userId)
    {
        var command = new LikePostCommand(userId, postId);
        var result = await mediator.SendCommandAsync<LikePostCommand, PostLikeResponseDto>(command);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [HttpDelete("{likeId}")]
    public async Task<ActionResult> DislikePost([FromRoute]string postId, [FromRoute]string likeId, [FromQuery]string userId)
    {
        var command = new DislikePostCommand(postId, likeId, userId);
        await mediator.SendCommandAsync<DislikePostCommand, Unit>(command);
        return Ok();
    }
}