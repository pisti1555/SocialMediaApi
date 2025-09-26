using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(1000).WithMessage("Text is too long");
    }
}