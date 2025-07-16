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
                opt => opt.MapFrom(x => x.User.UserName));
        CreateMap<PostComment, PostCommentResponseDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.User.UserName));
        CreateMap<PostLike, PostLikeResponseDto>()
            .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.User.UserName));
    }
}