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
    public ICollection<Post> Posts = [];
    public ICollection<PostComment> PostComments = [];
    public ICollection<PostLike> PostLikes = [];
    
    public ICollection<Friendship> Friends = [];
    
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