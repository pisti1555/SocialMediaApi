using Application.Responses;
using AutoMapper;
using Domain.Users;

namespace Application.Common.Mappings;

public class FriendshipProfile : Profile
{
    public FriendshipProfile()
    {
        CreateMap<Friendship, FriendshipResponseDto>()
            .ForMember(x => x.RequesterUserName, opt => opt.MapFrom(x => x.Requester.UserName))
            .ForMember(x => x.ResponderUserName, opt => opt.MapFrom(x => x.Responder.UserName));
    }
}