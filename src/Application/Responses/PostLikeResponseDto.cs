namespace Application.Responses;

public class PostLikeResponseDto
{
    public required Guid Id { get; init; }
    public required Guid PostId { get; init; }
    public required Guid UserId { get; init; }
    
    public required string UserName { get; init; }
}