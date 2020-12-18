using AuthorizationManagement.Api.Models.Internal;
using AutoMapper;

namespace AuthorizationManagement.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Models.Application, Application>()
                .ReverseMap();

            CreateMap<Models.Group, Group>()
                .ReverseMap();

            CreateMap<Models.User, User>()
                .ReverseMap();

            CreateMap<Models.UserGroup, UserGroup>()
                .ReverseMap();
        }
    }
}