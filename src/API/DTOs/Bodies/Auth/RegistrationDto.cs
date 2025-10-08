namespace API.DTOs.Bodies.Auth;

public record RegistrationDto(
    string? UserName, 
    string? Email, 
    string? Password,
    string? FirstName, 
    string? LastName, 
    string? DateOfBirth,
    bool? RememberMe
);