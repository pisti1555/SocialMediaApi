using FluentValidation;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostValidator : AbstractValidator<DislikePostCommand>
{
    public DislikePostValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required");
        
        RuleFor(x => x.PostId).NotEmpty().WithMessage("PostId is required");
        RuleFor(x => x.PostId).NotNull().WithMessage("PostId is required");
    }
}