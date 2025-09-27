using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostValidator : AbstractValidator<DislikePostCommand>
{
    public DislikePostValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();
    }
}