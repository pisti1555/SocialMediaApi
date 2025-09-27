using Domain.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Domain.Posts;

public class PostComment : EntityBase
{
    public string Text { get; private set; } = string.Empty;

    // Realtionships
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; }
    public Guid PostId { get; private set; }
    public Post Post { get; private set; }

    // Constructors   
    protected PostComment() {}
    internal PostComment(string text, AppUser user, Post post) 
    {
        Text = text;
        User = user;
        UserId = user.Id;
        Post = post;
        PostId = post.Id;
    }
    
    public void UpdateText(string text, Post post)
    {
        if (text == Text) return;
        
        if (string.IsNullOrWhiteSpace(text)) throw new BadRequestException("Text cannot be empty.");
        if (text.Length > 1000) throw new BadRequestException("Text is too long.");
        
        Text = text;
        post.UpdateLastInteraction();
    }
}