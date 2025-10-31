using Domain.Common;
using Domain.Common.Exceptions.CustomExceptions;

namespace Domain.Users;

public class Friendship : EntityBase
{
    public Guid RequesterId { get; private set; }
    public AppUser Requester { get; private set; }

    public Guid ResponderId { get; private set; }
    public AppUser Responder { get; private set; }

    public bool IsConfirmed { get; private set; }
    
    protected internal Friendship() { }
    internal Friendship(Guid requesterId, Guid responderId, AppUser? requester = null, AppUser? responder = null)
    {
        RequesterId = requesterId;
        ResponderId = responderId;

        if (requester is not null) Requester = requester;
        if (responder is not null) Responder = responder;
        
        IsConfirmed = false;
    }

    public void Confirm(Guid responderId)
    {
        if (ResponderId == responderId)
        {
            IsConfirmed = true;
        }
        else
        {
            throw new BadRequestException("You cannot accept or decline this friendship.");
        }
    }
}