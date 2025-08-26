using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostValidator()
    {
        RuleFor(x => x.PostId)
            .NotNull().WithMessage("PostId is required")
            .NotEmpty().WithMessage("PostId is required");
        
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required")
            .NotEmpty().WithMessage("UserId is required");
        
        RuleFor(x => x.Text)
            .NotNull().WithMessage("Text is required")
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(1000).WithMessage("Text is too long");
    }
}