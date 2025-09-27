using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Domain.Users.Factories;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public class CreateUserHandler(
    IRepository<AppUser, UserResponseDto> repository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<CreateUserCommand, UserResponseDto>
{
    private const int MinAge = 13;
    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await ThrowIfUserAlreadyExistsByUsername(request.UserName);
        await ThrowIfUserAlreadyExistsByEmail(request.Email);
        
        var dob = Parser.ParseDateOrThrow(request.DateOfBirth);
        ValidateDateOfBirth(dob);
        
        var user = AppUserFactory.Create(
            request.UserName, request.Email, request.FirstName, request.LastName, dob
        );
        
        repository.Add(user);
        
        if (!await repository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("User could not be created.");
        
        var userResponseDto = mapper.Map<UserResponseDto>(user);
        
        await cache.SetAsync($"user-{user.Id.ToString()}", userResponseDto, cancellationToken);

        return userResponseDto;
    }

    private async Task ThrowIfUserAlreadyExistsByUsername(string username)
    {
        var exists = await repository.ExistsAsync(x => x.UserName == username);
        if (exists)
            throw new BadRequestException("Username already exists.");
    }
    private async Task ThrowIfUserAlreadyExistsByEmail(string email)
    {
        var exists = await repository.ExistsAsync(x => x.Email == email);
        if (exists)
            throw new BadRequestException("Email already exists.");
    }

    private void ValidateDateOfBirth(DateOnly dateOfBirth)
    {
        if (dateOfBirth == default)
            throw new BadRequestException("Date of birth is required.");
        
        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.Now))
            throw new BadRequestException("Date of birth is invalid.");
        
        if (dateOfBirth < DateOnly.FromDateTime(new DateTime(1900, 1, 1)))
            throw new BadRequestException("Date of birth is invalid.");
        
        if (DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-MinAge)) < dateOfBirth)
            throw new BadRequestException("Minimum age is 13.");
    }
}