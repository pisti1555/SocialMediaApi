using Domain.Posts.Validators;
using Domain.Users;

namespace Domain.Posts.Factories;

public static class PostCommentFactory
{
    public static PostComment Create(string text, AppUser user, Post post, bool byPassValidation = false)
    {
        return byPassValidation ? 
            new PostComment(text, user, post) : 
            CreateWithValidation(text, user, post);
    }

    private static PostComment CreateWithValidation(string text, AppUser user, Post post)
    {
        PostCommentValidator.ValidateText(text);
        PostCommentValidator.ValidateUser(user);
        PostCommentValidator.ValidatePost(post);
        
        return new PostComment(text, user, post);
    }
}