using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, UserLoginData>();
            CreateMap<User, UserData>();
            CreateMap<User, UserDataExport>();
        }
    }
}
