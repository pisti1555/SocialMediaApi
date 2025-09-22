using Domain.Common.Exceptions.CustomExceptions;
using UnitTests.Factories;

namespace UnitTests.Domain.PostTests;

public class RootTests
{
    [Fact]
    public void UpdateText_ShouldUpdateTextAndLastInteraction()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var prevText = post.Text;
        var prevLastInteraction = post.LastInteraction;
        
        // Act
        post.UpdateText("Updated text");
        
        // Assert
        Assert.NotEqual(prevText, post.Text);
        Assert.NotEqual(prevLastInteraction, post.LastInteraction);
    }
    
    [Fact]
    public void UpdateText_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var prevText = post.Text;
        var prevLastInteraction = post.LastInteraction;
        
        // Act & Assert
        Assert.Throws<BadRequestException>(() => post.UpdateText(new string('a', 20001)));

        Assert.Equal(prevText, post.Text);
        Assert.Equal(prevLastInteraction, post.LastInteraction);
    }
}