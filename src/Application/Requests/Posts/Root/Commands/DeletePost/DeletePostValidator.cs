using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
    }
}