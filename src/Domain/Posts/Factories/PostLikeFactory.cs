using Domain.Posts.Validators;
using Domain.Users;

namespace Domain.Posts.Factories;

public static class PostLikeFactory
{
    public static PostLike Create(AppUser user, Post post, bool byPassValidation = false)
    {
        return byPassValidation ? 
            new PostLike(user, post) : 
            CreateWithValidation(user, post);
    }

    private static PostLike CreateWithValidation(AppUser user, Post post)
    {
        PostLikeValidator.ValidateUser(user);
        PostLikeValidator.ValidatePost(post);
        
        return new PostLike(user, post);
    }
}