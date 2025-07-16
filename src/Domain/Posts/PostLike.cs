using Domain.Common;
using Domain.Users;

namespace Domain.Posts;

public class PostLike : BaseEntity
{
    public Guid UserId;
    public AppUser User { get; private set; }
    public Guid PostId;
    public Post Post { get; private set; }

    // Constructors
    protected PostLike() {}
    internal PostLike(AppUser user, Post post) 
    {
        User = user;
        Post = post;
    }
}