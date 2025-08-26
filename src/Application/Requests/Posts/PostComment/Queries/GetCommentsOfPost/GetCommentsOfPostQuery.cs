using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public record GetCommentsOfPostQuery(string PostId) : IQuery<List<PostCommentResponseDto>>;