using Domain.Posts.Validators;
using Domain.Users;

namespace Domain.Posts.Factories;

public static class PostFactory
{
    public static Post Create(string text, AppUser user, bool byPassValidation = false)
    {
        return byPassValidation ? 
            new Post(text, user) : 
            CreateWithValidation(text, user);
    }

    private static Post CreateWithValidation(string text, AppUser user)
    {
        PostValidator.ValidateText(text);
        PostValidator.ValidateUser(user);

        return new Post(text, user);
    }
}