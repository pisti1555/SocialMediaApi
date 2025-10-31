using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Friends.Queries.GetAllOfUser;

public class GetFriendsOfUserHandler(
    IRepository<AppUser> userRepository,
    IRepository<Friendship, FriendshipResponseDto> friendshipRepository) : IQueryHandler<GetFriendsOfUserQuery, List<FriendshipResponseDto>>
{
    public async Task<List<FriendshipResponseDto>> Handle(GetFriendsOfUserQuery query, CancellationToken cancellationToken)
    {
        var uid = Parser.ParseIdOrThrow(query.UserId);
        await ThrowIfUserDoesNotExist(uid);
        
        return await friendshipRepository
            .GetAllAsync(x => x.RequesterId == uid || x.ResponderId == uid && x.IsConfirmed, cancellationToken);
    }

    private async Task ThrowIfUserDoesNotExist(Guid id)
    {
        var exists = await userRepository.ExistsAsync(id);
        if (!exists) throw new NotFoundException("User not found.");
    }
}