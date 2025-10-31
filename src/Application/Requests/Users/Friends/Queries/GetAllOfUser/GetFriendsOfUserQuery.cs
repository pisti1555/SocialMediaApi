using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Users.Friends.Queries.GetAllOfUser;

public record GetFriendsOfUserQuery(string UserId) : IQuery<List<FriendshipResponseDto>>;