using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;
using System.Linq;

namespace AuthorizationManagement.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Models.Application, Application>()
                .ForMember(g => g.ETag, cfg => cfg.Ignore())
                .ForMember(g => g.LastModifiedOn, cfg => cfg.Ignore())
                .ReverseMap();

            CreateMap<Models.Group, Group>()
                .ForMember(g => g.ApplicationId, cfg => cfg.Ignore())
                .ForMember(g => g.ETag, cfg => cfg.Ignore())
                .ForMember(g => g.LastModifiedOn, cfg => cfg.Ignore())
                .ReverseMap();

            CreateMap<Models.User, User>()
                .ForMember(u => u.Groups, cfg => cfg.MapFrom(user => user.Groups.Select(u => u.Id).ToList()))
                .ForMember(u => u.ApplicationId, cfg => cfg.Ignore())
                .ForMember(u => u.ETag, cfg => cfg.Ignore())
                .ForMember(u => u.LastModifiedOn, cfg => cfg.Ignore());

            CreateMap<User, Models.User>()
                .ForMember(u => u.Groups, cfg => cfg.Ignore());
            
            CreateMap<User, Models.UserInfo>();
        }
    }
}