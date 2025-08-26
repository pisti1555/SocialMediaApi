using Domain.Common;
using Domain.Posts;

namespace Domain.Users;

public class AppUser : EntityBase
{
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public DateTime LastActive { get; private set; } = DateTime.UtcNow;
    
    // Relationships
    private List<Post> _posts = [];
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();
    
    private List<PostComment> _postComments = [];
    public IReadOnlyCollection<PostComment> PostComments => _postComments.AsReadOnly();
    
    private List<PostLike> _postLikes = [];
    public IReadOnlyCollection<PostLike> PostLikes => _postLikes.AsReadOnly();
    
    // Constructors
    protected internal AppUser() { }
    internal AppUser(string userName, string email, string firstName, string lastName, DateOnly dateOfBirth)
    {
        UserName = userName;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }
    
    // Methods
    public void UpdateLastActive() => LastActive = DateTime.UtcNow;
}