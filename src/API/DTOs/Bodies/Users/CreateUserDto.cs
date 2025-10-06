namespace API.DTOs.Bodies.Users;

public record CreateUserDto(
    string? UserName, 
    string? Email, 
    string? Password,
    string? FirstName, 
    string? LastName, 
    string? DateOfBirth
);