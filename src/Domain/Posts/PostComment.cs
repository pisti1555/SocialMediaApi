using Domain.Common;
using Domain.Users;

namespace Domain.Posts;

public class PostComment : BaseEntity
{
    public string Text { get; private set; } = string.Empty;

    // Realtionships
    public Guid UserId;
    public AppUser User { get; private set; }
    public Guid PostId;
    public Post Post { get; private set; }

    // Constructors   
    protected PostComment() {}
    internal PostComment(string text, AppUser user, Post post) 
    {
        Text = text;
        User = user;
        Post = post;
    }
}