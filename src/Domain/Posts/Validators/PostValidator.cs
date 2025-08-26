using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Domain.Posts.Validators;

internal static class PostValidator
{
    internal static void ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new BadRequestException("Text is required.");
        if (text.Length > 20000)
            throw new BadRequestException("Text is too long.");
    }
    
    internal static void ValidateUser(AppUser? user)
    {
        if (user is null)
            throw new BadRequestException("User is required.");
    }
}