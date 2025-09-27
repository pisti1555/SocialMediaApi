using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostValidator : AbstractValidator<RemoveCommentFromPostCommand>
{
    public RemoveCommentFromPostValidator()
    {
        RuleFor(x => x.PostId)
            .NotNull().WithMessage("PostId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("CommentId is required")
            .MustBeParsableGuid();
    }
}