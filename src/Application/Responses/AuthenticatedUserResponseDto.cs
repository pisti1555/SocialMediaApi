namespace Application.Responses;

public class AuthenticatedUserResponseDto : UserResponseDto
{
    public required string Token { get; set; }
}