using API.Controllers.Common;
using API.DTOs.Params;
using API.Extensions;
using Application.Common.Pagination;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Requests.Users.Root.Queries.GetById;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/users")]
public class UserController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetById([FromRoute]string id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery(id);
        var result = await mediator.SendQueryAsync<GetUserByIdQuery, UserResponseDto>(query, ct);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAllPaged([FromQuery]PaginationParams pagination, CancellationToken ct)
    {
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        
        var result = await mediator.SendQueryAsync<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>(query, ct);
        Response.AddPaginationHeaders(result);
        
        return Ok(result);
    }
}