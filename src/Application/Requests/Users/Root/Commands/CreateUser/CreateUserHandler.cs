using Application.Common.Interfaces.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Users.Factories;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public class CreateUserHandler(IAppUserRepository userRepository, IMapper mapper) : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await ThrowIfUserAlreadyExistsByUsername(request.UserName);
        await ThrowIfUserAlreadyExistsByEmail(request.Email);
        
        var user = AppUserFactory.Create(
            request.UserName, request.Email, request.FirstName, request.LastName, request.DateOfBirth
        );
        
        userRepository.Add(user);
        
        if (!await userRepository.SaveChangesAsync())
            throw new BadRequestException("User could not be created.");

        return mapper.Map<UserResponseDto>(user);
    }

    private async Task ThrowIfUserAlreadyExistsByUsername(string username)
    {
        if (await userRepository.ExistsByUsernameAsync(username))
            throw new BadRequestException("Username already exists.");
    }
    private async Task ThrowIfUserAlreadyExistsByEmail(string email)
    {
        if (await userRepository.ExistsByEmailAsync(email))
            throw new BadRequestException("Email already exists.");
    }
}