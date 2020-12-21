using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;

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
                .ForMember(u => u.ApplicationId, cfg => cfg.Ignore())
                .ForMember(u => u.ETag, cfg => cfg.Ignore())
                .ForMember(u => u.LastModifiedOn, cfg => cfg.Ignore())
                .ReverseMap();

            CreateMap<Models.UserGroup, UserGroup>()
                .ForMember(ug => ug.ApplicationId, cfg => cfg.Ignore())
                .ForMember(ug => ug.ETag, cfg => cfg.Ignore())
                .ForMember(ug => ug.LastModifiedOn, cfg => cfg.Ignore())
                .ReverseMap();
        }
    }
}