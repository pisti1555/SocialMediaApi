using Application.Responses;
using AutoMapper;
using Domain.Users;

namespace Application.Common.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<AppUser, UserResponseDto>();
        CreateMap<AppUser, AuthenticatedUserResponseDto>()
            .ForMember(x => x.AccessToken, opt => opt.Ignore())
            .ForMember(x => x.RefreshToken, opt => opt.Ignore());
    }
}