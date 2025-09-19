using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

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

        var cachedUser = await cache.GetAsync<UserResponseDto>(cacheKey, cancellationToken);
        if (cachedUser is not null) return cachedUser;
        
        var user = await userRepository.GetByIdAsync(guid);
        if (user is null) throw new NotFoundException("User not found.");
        
        var userResponseDto = mapper.Map<UserResponseDto>(user);
        
        await cache.SetAsync(cacheKey, userResponseDto, cancellationToken);
        
        return userResponseDto;
    }
}