namespace API.DTOs.Bodies.Auth;

public record RefreshAccessDto(
    string? AccessToken,
    string? RefreshToken    
);