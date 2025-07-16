using Domain.Common;
using Domain.Posts.Validators;
using Domain.Users;

namespace Domain.Posts;

public class Post : BaseEntity, IEntityRoot
{
    public string Text { get; private set; } = string.Empty;

    public ICollection<PostLike> Likes { get; private set; } = [];
    public ICollection<PostComment> Comments { get; private set; } = [];
    public DateTime LastInteraction { get; private set; } = DateTime.UtcNow;
    
    public Guid AppUserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    
    // Constructors
    protected Post() { }
    internal Post(string text, AppUser user)
    {
        Text = text;
        User = user;
    }

    // Functions
    public void AddLike(PostLike like)
    {
        PostLikeValidator.ValidateUser(like.User);
        PostLikeValidator.ValidatePost(like.Post);
        
        Likes.Add(like);
        LastInteraction = DateTime.UtcNow;
    }
    public void RemoveLike(PostLike like)
    {
        PostLikeValidator.ValidateUser(like.User);
        PostLikeValidator.ValidatePost(like.Post);
        
        Likes.Remove(like);
        LastInteraction = DateTime.UtcNow;
    }
    

    public void AddComment(PostComment comment)
    {
        PostCommentValidator.ValidateUser(comment.User);
        PostCommentValidator.ValidatePost(comment.Post);
        PostCommentValidator.ValidateText(comment.Text);
        
        Comments.Add(comment);
        LastInteraction = DateTime.UtcNow;
    }
    public void RemoveComment(PostComment comment)
    {
        PostCommentValidator.ValidateUser(comment.User);
        PostCommentValidator.ValidatePost(comment.Post);
        
        Comments.Remove(comment);
        LastInteraction = DateTime.UtcNow;
    }
}