using Domain.Common;
using Domain.Users;

namespace Domain.Posts;

public class Post : EntityBase
{
    public string Text { get; private set; } = string.Empty;

    private readonly List<PostLike> _likes = [];
    public IReadOnlyCollection<PostLike> Likes => _likes.AsReadOnly();
    

    private readonly List<PostComment> _comments = [];
    public IReadOnlyCollection<PostComment> Comments => _comments.AsReadOnly();
    
    public DateTime LastInteraction { get; private set; } = DateTime.UtcNow;
    
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    
    // Constructors
    protected Post() { }
    internal Post(string text, AppUser user)
    {
        Text = text;
        User = user;
        UserId = user.Id;
    }
    
    // Methods
    public void UpdateLastInteraction() => LastInteraction = DateTime.UtcNow;
}