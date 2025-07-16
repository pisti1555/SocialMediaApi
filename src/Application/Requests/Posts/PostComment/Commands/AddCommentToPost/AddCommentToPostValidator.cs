using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostValidator()
    {
        RuleFor(x => x.PostId).NotEmpty().WithMessage("PostId is required");
        RuleFor(x => x.PostId).NotNull().WithMessage("PostId is required");
        
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required");
        
        RuleFor(x => x.Text).NotEmpty().WithMessage("Text is required");
        RuleFor(x => x.Text).NotNull().WithMessage("Text is required");
        RuleFor(x => x.Text).MaximumLength(1000).WithMessage("Text is too long");
    }
}