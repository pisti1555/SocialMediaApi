using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Friends.Commands.DeleteFriend;

public class DeleteFriendHandler(
    IRepository<AppUser> userRepository, 
    IRepository<Friendship> friendshipRepository
) : ICommandHandler<DeleteFriendCommand, Unit>
{
    public async Task<Unit> Handle(DeleteFriendCommand command, CancellationToken cancellationToken)
    {
        var uid = Parser.ParseIdOrThrow(command.CurrentUserId);
        var friendshipId = Parser.ParseIdOrThrow(command.FriendshipId);
        
        await ThrowIfUserDoesNotExist(uid);
        var friendship = await GetFriendshipOrThrow(friendshipId);

        ThrowIfUserDoesNotOwnFriendship(uid, friendship);
        
        friendshipRepository.Delete(friendship);
        if (!await friendshipRepository.SaveChangesAsync(cancellationToken))
        {
            throw new BadRequestException("Friendship could not be deleted.");
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
    
    private static void ThrowIfUserDoesNotOwnFriendship(Guid userId, Friendship friendship)
    {
        var ownsFriendship = userId == friendship.RequesterId || userId == friendship.ResponderId;
        if (!ownsFriendship) throw new BadRequestException("You do not own this friendship.");
    }
}