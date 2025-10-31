using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Friends.Commands.DeleteFriend;

public record DeleteFriendCommand(string CurrentUserId, string FriendshipId) : ICommand<Unit>;