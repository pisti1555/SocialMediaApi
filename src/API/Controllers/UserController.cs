using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Requests.Users.Root.Queries.GetById;
using Application.Responses;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody]CreateUserCommand command)
    {
        var result = await mediator.SendCommandAsync<CreateUserCommand, UserResponseDto>(command);
        return CreatedAtAction(nameof(GetById), new {id = result.Id}, result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetById([FromRoute]string id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await mediator.SendQueryAsync<GetUserByIdQuery, UserResponseDto>(query);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAllPaged([FromQuery]PaginationParams pagination)
    {
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.SendQueryAsync<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>(query);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
}