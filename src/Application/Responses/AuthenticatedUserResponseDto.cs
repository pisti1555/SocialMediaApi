namespace Application.Responses;

public class AuthenticatedUserResponseDto : UserResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}