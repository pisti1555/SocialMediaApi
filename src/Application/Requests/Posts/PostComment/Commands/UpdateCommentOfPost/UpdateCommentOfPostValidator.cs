using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;

public class UpdateCommentOfPostValidator : AbstractValidator<UpdateCommentOfPostCommand>
{
    public UpdateCommentOfPostValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("CommentId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(1000).WithMessage("Text is too long");
    }
}