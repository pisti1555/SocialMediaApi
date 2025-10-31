namespace Domain.Users.Factories;

public static class FriendshipFactory
{
    public static Friendship Create(AppUser requester, AppUser responder)
    {
        return new Friendship(requester.Id, responder.Id, requester, responder);
    }
    
    public static Friendship Create(Guid requesterId, Guid responderId)
    {
        return new Friendship(requesterId, responderId);
    }
}