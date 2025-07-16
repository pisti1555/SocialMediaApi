namespace Application.Responses;

public class PostCommentResponseDto
{
    public required Guid Id { get; init; }
    
    public required Guid UserId { get; init; }
    public required Guid PostId { get; init; }
    
    public required string UserName { get; init; }
    public required string Text { get; init; }
}