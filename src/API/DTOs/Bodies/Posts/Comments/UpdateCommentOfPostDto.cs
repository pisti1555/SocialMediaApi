namespace API.DTOs.Bodies.Posts.Comments;

public record UpdateCommentOfPostDto(
    string? UserId, 
    string? Text
);