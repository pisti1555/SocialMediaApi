using API.Controllers.Common;
using API.DTOs.Bodies.Posts.Root;
using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Posts.Root.Commands.CreatePost;
using Application.Requests.Posts.Root.Commands.DeletePost;
using Application.Requests.Posts.Root.Commands.UpdatePost;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Requests.Posts.Root.Queries.GetById;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/posts")]
public class PostController(IMediator mediator) : BaseApiController
{
    [Authorize]
    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> Create([FromBody]CreatePostDto dto, CancellationToken ct)
    {
        var command = new CreatePostCommand(
            dto.Text ?? string.Empty, 
            User.GetUserId()
        );
        
        var result = await mediator.SendCommandAsync<CreatePostCommand, PostResponseDto>(command, ct);
        return CreatedAtAction(nameof(GetById), new {id = result.Id}, result);
    }

    [Authorize]
    [MapToApiVersion(1)]
    [HttpPatch("{id}")]
    public async Task<ActionResult<PostResponseDto>> Update([FromRoute]string id, [FromBody]UpdatePostDto dto, CancellationToken ct)
    {
        var command = new UpdatePostCommand(
            id, 
            User.GetUserId(),
            dto.Text ?? string.Empty
        );
        
        var result = await mediator.SendCommandAsync<UpdatePostCommand, PostResponseDto>(command, ct);
        return Ok(result);
    }
    
    [Authorize]
    [MapToApiVersion(1)]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]string id, CancellationToken ct)
    {
        var command = new DeletePostCommand(id, User.GetUserId());
        await mediator.SendCommandAsync<DeletePostCommand, Unit>(command, ct);
        return Ok();
    }
    
    [MapToApiVersion(1)]
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetById([FromRoute]string id, CancellationToken ct)
    {
        var query = new GetPostByIdQuery(id);
        var result = await mediator.SendQueryAsync<GetPostByIdQuery, PostResponseDto>(query, ct);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostResponseDto>>> GetAllPaged([FromQuery]PaginationParams pagination, CancellationToken ct)
    {
        var query = new GetAllPostsPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.SendQueryAsync<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>(query, ct);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
}