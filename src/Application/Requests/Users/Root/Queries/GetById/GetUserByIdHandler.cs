using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandler(IAppUserRepository userRepository, IMapper mapper) : IQueryHandler<GetUserByIdQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var user = await userRepository.GetByIdAsync(guid);
        if (user is null)
            throw new NotFoundException("User not found.");
        
        return mapper.Map<UserResponseDto>(user);
    }
}