using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required");
        
        RuleFor(x => x.Text).NotEmpty().WithMessage("Text is required");
        RuleFor(x => x.Text).NotNull().WithMessage("Text is required");
        RuleFor(x => x.Text).MaximumLength(20000).WithMessage("Text is too long");
    }
}