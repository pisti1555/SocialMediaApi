using Domain.Users;
using Shared.Exceptions.CustomExceptions;

namespace Domain.Posts.Validators;

internal static class PostLikeValidator
{
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