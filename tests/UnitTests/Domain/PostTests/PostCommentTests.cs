using Domain.Common.Exceptions.CustomExceptions;
using UnitTests.Factories;

namespace UnitTests.Domain.PostTests;

public class PostCommentTests
{
    [Fact]
    public void UpdateText_ShouldUpdateTextAndLastInteraction()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var comment = TestDataFactory.CreateComment(post);
        var prevText = comment.Text;
        var prevLastInteraction = post.LastInteraction;
        
        // Act
        comment.UpdateText("Updated comment text", post);
        
        // Assert
        Assert.NotEqual(prevText, comment.Text);
        Assert.NotEqual(prevLastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void UpdateText_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var comment = TestDataFactory.CreateComment(post);
        var prevText = comment.Text;
        var prevLastInteraction = post.LastInteraction;
        
        // Act & Assert
        Assert.Throws<BadRequestException>(() => comment.UpdateText(new string('a', 1001), post));

        Assert.Equal(prevText, comment.Text);
        Assert.Equal(prevLastInteraction, post.LastInteraction);
    }
}