using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Users.Friends.Commands.DeleteFriend;

public class DeleteFriendValidator : AbstractValidator<DeleteFriendCommand>
{
    public DeleteFriendValidator()
    {
        RuleFor(x => x.CurrentUserId )
            .NotEmpty().WithMessage("Current User Id is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.FriendshipId)
            .NotEmpty().WithMessage("Friendship Id is required")
            .MustBeParsableGuid();
    }
}