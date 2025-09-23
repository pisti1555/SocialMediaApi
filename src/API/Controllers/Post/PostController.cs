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
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/posts")]
public class PostController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> Create([FromBody]CreatePostDto dto)
    {
        var command = new CreatePostCommand(dto.Text, dto.UserId);
        var result = await mediator.SendCommandAsync<CreatePostCommand, PostResponseDto>(command);
        return CreatedAtAction(nameof(GetById), new {id = result.Id}, result);
    }

    [MapToApiVersion(1)]
    [HttpPatch("{id}")]
    public async Task<ActionResult<PostResponseDto>> Update([FromRoute]string id, [FromBody]UpdatePostDto dto)
    {
        var command = new UpdatePostCommand(id, dto.UserId, dto.Text);
        var result = await mediator.SendCommandAsync<UpdatePostCommand, PostResponseDto>(command);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]string id, [FromQuery]string userId)
    {
        var command = new DeletePostCommand(id, userId);
        await mediator.SendCommandAsync<DeletePostCommand, Unit>(command);
        return Ok();
    }
    
    [MapToApiVersion(1)]
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetById([FromRoute]string id)
    {
        var query = new GetPostByIdQuery(id);
        var result = await mediator.SendQueryAsync<GetPostByIdQuery, PostResponseDto>(query);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
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
}