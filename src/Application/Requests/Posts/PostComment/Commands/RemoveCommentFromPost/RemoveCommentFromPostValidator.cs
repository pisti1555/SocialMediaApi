using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostValidator : AbstractValidator<RemoveCommentFromPostCommand>
{
    public RemoveCommentFromPostValidator()
    {
        RuleFor(x => x.PostId).NotEmpty().WithMessage("PostId is required");
        RuleFor(x => x.PostId).NotNull().WithMessage("PostId is required");
        
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required");
        
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required");
        RuleFor(x => x.CommentId).NotNull().WithMessage("CommentId is required");
    }
}