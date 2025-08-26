namespace Application.Responses;

public class PostResponseDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Text { get; init; }
    public required DateTime CreatedAt { get; init; }
    
    public required List<PostCommentResponseDto> Comments { get; init; }
    public required int CommentsCount { get; init; }
    
    public required List<PostLikeResponseDto> Likes { get; init; }
    public required int LikesCount { get; init; }
}