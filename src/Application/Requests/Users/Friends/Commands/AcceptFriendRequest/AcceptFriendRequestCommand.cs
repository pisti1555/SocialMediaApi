using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Friends.Commands.AcceptFriendRequest;

public record AcceptFriendRequestCommand(string CurrentUserId, string FriendshipId) : ICommand<Unit>;