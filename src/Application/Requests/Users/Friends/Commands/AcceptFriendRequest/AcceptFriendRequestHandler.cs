using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Friends.Commands.AcceptFriendRequest;

public class AcceptFriendRequestHandler(
    IRepository<AppUser> userRepository,
    IRepository<Friendship> friendshipRepository    
) : ICommandHandler<AcceptFriendRequestCommand, Unit>
{
    public async Task<Unit> Handle(AcceptFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var uid = Parser.ParseIdOrThrow(command.CurrentUserId);
        var friendshipId = Parser.ParseIdOrThrow(command.FriendshipId);

        await ThrowIfUserDoesNotExist(uid);
        var friendship = await GetFriendshipOrThrow(friendshipId);

        if (friendship.ResponderId != uid)
        {
            throw new BadRequestException("You cannot accept or decline this friendship.");
        }

        if (friendship.IsConfirmed)
        {
            throw new BadRequestException("This friendship is already confirmed.");
        }

        friendship.Confirm(uid);
        
        friendshipRepository.Update(friendship);
        if (!await friendshipRepository.SaveChangesAsync(cancellationToken))
        {
            throw new BadRequestException("Friendship could not be updated.");
        }
        
        return Unit.Value;
    }

    private async Task ThrowIfUserDoesNotExist(Guid userId)
    {
        var exists = await userRepository.ExistsAsync(userId);
        if (!exists) throw new NotFoundException("User was not found.");
    }
    
    private async Task<Friendship> GetFriendshipOrThrow(Guid friendshipId)
    {
        var friendship = await friendshipRepository.GetByIdAsync(friendshipId);
        return friendship ?? throw new NotFoundException("Friendship was not found.");
    }
}