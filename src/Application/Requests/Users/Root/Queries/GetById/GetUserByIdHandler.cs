using Application.Common.Interfaces.Repositories;
using Application.Responses;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandler(IAppUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.Id);
        
        var user = await userRepository.GetDtoByIdAsync(guid);
        if (user is null)
            throw new NotFoundException("User not found.");
        
        return user;
    }
    
    private static Guid ParseGuid(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        if (!result)
            throw new BadRequestException("Cannot parse the id.");
        return guid;  
    }
}