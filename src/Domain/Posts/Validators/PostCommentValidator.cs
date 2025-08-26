using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Domain.Posts.Validators;

internal static class PostCommentValidator
{
    internal static void ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new BadRequestException("Text is required.");
        if (text.Length > 5000)
            throw new BadRequestException("Text is too long.");
    }
    
    internal static void ValidateUser(AppUser? user)
    {
        if (user is null)
            throw new BadRequestException("User is required.");
    }
    
    internal static void ValidatePost(Post? post)
    {
        if (post is null)
            throw new BadRequestException("Post is required.");
    }
}