using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostValidator : AbstractValidator<LikePostCommand>
{
    public LikePostValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MustBeParsableGuid();

        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required")
            .MustBeParsableGuid();
    }
}