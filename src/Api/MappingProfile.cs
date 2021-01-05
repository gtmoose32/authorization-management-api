using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
using System.Linq;

namespace AuthorizationManagement.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Models.Application, Application>(MemberList.None)
                .ReverseMap();

            CreateMap<Models.Group, Group>(MemberList.None)
                .ReverseMap();

            CreateMap<Models.User, User>(MemberList.None)
                .ForMember(u => u.Groups, cfg => cfg.MapFrom(user => user.Groups.Select(u => u.Id).ToList()));

            CreateMap<User, Models.User>(MemberList.None)
                .ForMember(u => u.Groups, cfg => cfg.Ignore());
            
            CreateMap<User, Models.UserInfo>();
        }
    }
}