using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.UpdatePost;

public class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
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
            .MaximumLength(20000).WithMessage("Text is too long");
    }
}