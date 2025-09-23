using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandler(
    IRepository<AppUser, UserResponseDto> userRepository,
    ICacheService cache
) : IQueryHandler<GetUserByIdQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var cacheKey = $"user-{request.Id}";

        var cachedUser = await cache.GetAsync<UserResponseDto>(cacheKey, cancellationToken);
        if (cachedUser is not null) return cachedUser;
        
        var user = await userRepository.GetByIdAsync(guid, cancellationToken);
        if (user is null) throw new NotFoundException("User not found.");
        
        await cache.SetAsync(cacheKey, user, cancellationToken);
        
        return user;
    }
}