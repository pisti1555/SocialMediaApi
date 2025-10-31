using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Users.Friends.Queries.GetAllRequestsOfUser;

public record GetFriendRequestsOfUserQuery(string UserId) : IQuery<List<FriendshipResponseDto>>;