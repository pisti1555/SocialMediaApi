using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Requests.Users.Root.Queries.GetById;
using Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody]CreateUserCommand command)
    {
        return Ok(await mediator.Send(command));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetById([FromRoute]string id)
    {
        var query = new GetUserByIdQuery(id);
        return Ok(await mediator.Send(query));
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAllPaged([FromQuery]PaginationParams pagination)
    {
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.Send(query);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
}