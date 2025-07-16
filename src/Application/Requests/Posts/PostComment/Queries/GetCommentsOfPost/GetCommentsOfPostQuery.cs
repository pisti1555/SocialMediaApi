using System.Collections.Generic;
using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public record GetCommentsOfPostQuery(string PostId) : IRequest<List<PostCommentResponseDto>>;