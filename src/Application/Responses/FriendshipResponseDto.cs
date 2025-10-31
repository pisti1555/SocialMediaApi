namespace Application.Responses;

public record FriendshipResponseDto(
    Guid Id, 
    Guid RequesterId, string RequesterUserName,
    Guid ResponderId, string ResponderUserName
);