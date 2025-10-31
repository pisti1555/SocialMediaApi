using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Domain.Users.Factories;

namespace Application.Requests.Users.Friends.Commands.SendFriendRequest;

public class SendFriendRequestHandler(
    IRepository<AppUser> userRepository,
    IRepository<Friendship> friendshipRepository
) : ICommandHandler<SendFriendRequestCommand, Unit>
{
    public async Task<Unit> Handle(SendFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = Parser.ParseIdOrThrow(command.CurrentUserId);
        var userToAddId = Parser.ParseIdOrThrow(command.UserToAddId);

        ThrowIfSameUser(currentUserId, userToAddId);
        
        await ThrowIfUserDoesNotExist(currentUserId);
        await ThrowIfUserDoesNotExist(userToAddId);
        
        await ThrowIfFriendshipExists(currentUserId, userToAddId);

        var newFriendship = FriendshipFactory.Create(currentUserId, userToAddId);
        
        friendshipRepository.Add(newFriendship);
        if (!await friendshipRepository.SaveChangesAsync(cancellationToken))
        {
            throw new BadRequestException("Friendship could not be created.");
        }

        return Unit.Value;
    }

    private static void ThrowIfSameUser(Guid user1Id, Guid user2Id)
    {
        if (user1Id == user2Id) throw new BadRequestException("You cannot send a friend request to yourself.");
    }
    
    private async Task ThrowIfFriendshipExists(Guid user1Id, Guid user2Id)
    {
        var existingFriendship = await friendshipRepository.ExistsAsync(x =>
                x.RequesterId == user1Id && x.ResponderId == user2Id ||
                x.ResponderId == user1Id && x.RequesterId == user2Id
        );
        
        if (existingFriendship) throw new BadRequestException("This friendship already exists.");
    }
    
    private async Task ThrowIfUserDoesNotExist(Guid userId)
    {
        if (!await userRepository.ExistsAsync(userId))
            throw new NotFoundException("One of users was not found.");
    }
}