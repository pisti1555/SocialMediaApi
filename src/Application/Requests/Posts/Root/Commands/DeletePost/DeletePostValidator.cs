using FluentValidation;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostValidator()
    {
        
    }
}