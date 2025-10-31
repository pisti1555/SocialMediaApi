using API.Controllers.Common;
using API.DTOs.Bodies.Users;
using API.Extensions;
using Application.Requests.Users.Friends.Commands.AcceptFriendRequest;
using Application.Requests.Users.Friends.Commands.DeleteFriend;
using Application.Requests.Users.Friends.Commands.SendFriendRequest;
using Application.Requests.Users.Friends.Queries.GetAllOfUser;
using Application.Requests.Users.Friends.Queries.GetAllRequestsOfUser;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Domain.Common.Exceptions.CustomExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/users/{userId}/friendships")]
public class FriendshipController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<List<FriendshipResponseDto>>> GetFriendsOfUser([FromRoute]string userId, CancellationToken ct)
    {
        var query = new GetFriendsOfUserQuery(userId);
        var result = await mediator.SendQueryAsync<GetFriendsOfUserQuery, List<FriendshipResponseDto>>(query, ct);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [Authorize]
    [HttpGet("requests")]
    public async Task<ActionResult<List<FriendshipResponseDto>>> GetFriendRequests([FromRoute]string userId, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId != userId)
        {
            throw new UnauthorizedException("User identity mismatches route user identifier");
        }
        
        var query = new GetFriendRequestsOfUserQuery(userId);
        var result = await mediator.SendQueryAsync<GetFriendRequestsOfUserQuery, List<FriendshipResponseDto>>(query, ct);
        return Ok(result);
    }
    
    [MapToApiVersion(1)]
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> AddFriend([FromRoute]string userId, [FromBody]AddFriendDto dto, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId != userId)
        {
            throw new UnauthorizedException("User identity mismatches route user identifier");
        }
        
        var command = new SendFriendRequestCommand(
            currentUserId, 
            dto.FriendUserId ?? string.Empty
        );
        
        await mediator.SendCommandAsync<SendFriendRequestCommand, Unit>(command, ct);
        return NoContent();
    }
    
    [MapToApiVersion(1)]
    [Authorize]
    [HttpPatch("{friendshipId}/accept")]
    public async Task<ActionResult> AcceptFriendRequest([FromRoute]string userId, [FromRoute]string friendshipId, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId != userId)
        {
            throw new UnauthorizedException("User identity mismatches route user identifier");
        }
        
        var command = new AcceptFriendRequestCommand(currentUserId, friendshipId);
        
        await mediator.SendCommandAsync<AcceptFriendRequestCommand, Unit>(command, ct);
        return NoContent();
    }
    
    [MapToApiVersion(1)]
    [Authorize]
    [HttpPatch("{friendshipId}/decline")]
    public async Task<ActionResult> DeclineFriendRequest([FromRoute]string userId, [FromRoute]string friendshipId, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId != userId)
        {
            throw new UnauthorizedException("User identity mismatches route user identifier");
        }
        
        var command = new DeleteFriendCommand(currentUserId, friendshipId);
        
        await mediator.SendCommandAsync<DeleteFriendCommand, Unit>(command, ct);
        return NoContent();
    }
    
    [MapToApiVersion(1)]
    [Authorize]
    [HttpDelete("{friendshipId}")]
    public async Task<ActionResult> DeleteFriend([FromRoute]string userId, [FromRoute]string friendshipId, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();
        
        if (currentUserId != userId)
        {
            throw new UnauthorizedException("User identity mismatches route user identifier");
        }
        
        var command = new DeleteFriendCommand(currentUserId, friendshipId);
        
        await mediator.SendCommandAsync<DeleteFriendCommand, Unit>(command, ct);
        return NoContent();
    }
}