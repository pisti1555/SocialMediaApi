using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Domain.Users.Factories;
using Shared.Exceptions.CustomExceptions;

namespace DomainUnitTests.PostTests;

public class PostTests
{
    [Fact]
    public void AddLikeTest()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var like = PostLikeFactory.Create(user, post);
        var lastInteraction = post.LastInteraction;
        
        post.AddLike(like);
        
        Assert.Single(post.Likes);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void AddLikeTest_Fails()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var like = PostLikeFactory.Create(user, null, true);
        var lastInteraction = post.LastInteraction;
        
        Assert.Throws<BadRequestException>(() => post.AddLike(like));
        Assert.Empty(post.Likes);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void RemoveLikeTest()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var like = PostLikeFactory.Create(user, post);
        post.AddLike(like);
        
        var lastInteraction = post.LastInteraction;
        
        post.RemoveLike(like);
        
        Assert.Empty(post.Likes);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void AddCommentTest()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var comment = PostCommentFactory.Create("Test text", user, post);
        var lastInteraction = post.LastInteraction;
        
        post.AddComment(comment);
        
        Assert.Single(post.Comments);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void AddCommentTest_Fails()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var comment = PostCommentFactory.Create("", user, post, true);
        var lastInteraction = post.LastInteraction;
        
        Assert.Throws<BadRequestException>(() => post.AddComment(comment));
        Assert.Empty(post.Comments);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void RemoveCommentTest()
    {
        var user = CreateUser();
        var post = CreatePost(user);

        var comment = PostCommentFactory.Create("Test text", user, post);
        post.Comments.Add(comment);
        
        var lastInteraction = post.LastInteraction;
        
        post.RemoveComment(comment);
        
        Assert.Empty(post.Comments);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    private static AppUser CreateUser()
    {
        return AppUserFactory.Create(
            "test", 
            "email@email.com", 
            "Test", "User", 
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        );
    }
    
    private static Post CreatePost(AppUser user)
    {
        return PostFactory.Create("Test text", user);    
    }
}