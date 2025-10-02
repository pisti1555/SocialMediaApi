namespace API.DTOs.Bodies.Users;

public record LoginDto(
    string? UserName,
    string? Password
);