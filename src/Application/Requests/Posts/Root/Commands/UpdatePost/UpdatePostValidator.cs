using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.UpdatePost;

public class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(20000).WithMessage("Text is too long");
    }
}