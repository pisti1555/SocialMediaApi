using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Friends.Commands.SendFriendRequest;

public record SendFriendRequestCommand(
    string CurrentUserId,
    string UserToAddId
) : ICommand<Unit>;