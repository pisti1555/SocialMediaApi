using FluentValidation;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostValidator : AbstractValidator<LikePostCommand>
{
    public LikePostValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required")
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.PostId)
            .NotNull().WithMessage("PostId is required")
            .NotEmpty().WithMessage("PostId is required");
    }
}