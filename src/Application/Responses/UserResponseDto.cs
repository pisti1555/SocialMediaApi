namespace Application.Responses;

public class UserResponseDto
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime LastActive { get; init; }
}