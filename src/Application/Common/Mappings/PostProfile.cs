using Application.Responses;
using AutoMapper;
using Domain.Posts;

namespace Application.Common.Mappings;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<Post, PostResponseDto>()
            .ForMember(x => x.UserName, 
                opt => opt.MapFrom(x => x.User.UserName))
            .ForMember(x => x.LikesCount,
                opt => opt.MapFrom(x => x.Likes.Count))
            .ForMember(x => x.CommentsCount,
                opt => opt.MapFrom(x => x.Comments.Count));
        
        CreateMap<PostComment, PostCommentResponseDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.User.UserName));
        
        CreateMap<PostLike, PostLikeResponseDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.User.UserName));
    }
}