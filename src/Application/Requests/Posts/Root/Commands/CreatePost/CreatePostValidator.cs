using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(20000).WithMessage("Text is too long");
    }
}