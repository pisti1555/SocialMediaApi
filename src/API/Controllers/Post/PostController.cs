using API.Controllers.Common;
using API.DTOs.Bodies.Posts.Root;
using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Posts.Root.Commands.CreatePost;
using Application.Requests.Posts.Root.Commands.DeletePost;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Requests.Posts.Root.Queries.GetById;
using Application.Responses;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Post;

[Route("api/posts")]
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
}