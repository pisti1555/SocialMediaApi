using Domain.Common;
using Domain.Users;

namespace Domain.Posts;

public class PostLike : EntityBase
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; }
    public Guid PostId { get; private set; }
    public Post Post { get; private set; }

    // Constructors
    protected PostLike() {}
    internal PostLike(AppUser user, Post post) 
    {
        User = user;
        UserId = user.Id;
        Post = post;
        PostId = post.Id;
    }
}