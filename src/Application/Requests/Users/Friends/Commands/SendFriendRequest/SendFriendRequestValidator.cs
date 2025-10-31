using Application.Common.Extensions;
using FluentValidation;

namespace Application.Requests.Users.Friends.Commands.SendFriendRequest;

public class SendFriendRequestValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestValidator()
    {
        RuleFor(x => x.CurrentUserId )
            .NotEmpty().WithMessage("Current User Id is required")
            .MustBeParsableGuid();
        
        RuleFor(x => x.UserToAddId)
            .NotEmpty().WithMessage("Id of user to add is required")
            .MustBeParsableGuid();
    }
}