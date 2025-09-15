using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandler(
    IAppUserRepository userRepository, 
    ICacheService cache,
    IMapper mapper) : IQueryHandler<GetUserByIdQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var cacheKey = $"user-{request.Id}";

        var cachedUser = await cache.GetAsync<AppUser>(cacheKey, cancellationToken);
        if (cachedUser is not null) return mapper.Map<UserResponseDto>(cachedUser);
        
        var user = await userRepository.GetByIdAsync(guid);
        if (user is null) throw new NotFoundException("User not found.");
        
        await cache.SetAsync(cacheKey, user, cancellationToken);
        
        return mapper.Map<UserResponseDto>(user);
    }
}