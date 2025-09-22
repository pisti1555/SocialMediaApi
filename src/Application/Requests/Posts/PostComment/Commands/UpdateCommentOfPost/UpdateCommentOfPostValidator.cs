using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;

public class UpdateCommentOfPostValidator : AbstractValidator<UpdateCommentOfPostCommand>
{
    public UpdateCommentOfPostValidator()
    {
        RuleFor(x => x.CommentId)
            .NotNull().WithMessage("CommentId is required")
            .NotEmpty().WithMessage("CommentId is required");
        
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required")
            .NotEmpty().WithMessage("UserId is required");
        
        RuleFor(x => x.Text)
            .NotNull().WithMessage("Text is required")
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(1000).WithMessage("Text is too long");
    }
}