using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostHandler(
    IPostRepository postRepository,
    IAppUserRepository userRepository,
    IMapper mapper
) : ICommandHandler<AddCommentToPostCommand, PostCommentResponseDto>
{
    public async Task<PostCommentResponseDto> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        
        var user = await userRepository.GetByIdAsync(userGuid);
        if (user is null) throw new BadRequestException("User not found.");
        
        var post = await postRepository.GetByIdAsync(postGuid);
        if (post is null) throw new BadRequestException("Post not found.");
        
        var comment = PostCommentFactory.Create(request.Text, user, post);
        
        post.UpdateLastInteraction();
        
        postRepository.CommentRepository.Add(comment);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Comment could not be created.");
        
        return mapper.Map<PostCommentResponseDto>(comment);
    }
}