namespace API.DTOs.Bodies.Auth;

public record LoginDto(
    string? UserName,
    string? Password,
    bool? RememberMe
);