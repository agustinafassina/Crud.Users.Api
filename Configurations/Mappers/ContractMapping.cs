
using AutoMapper;
using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;

namespace UsersApi.Configurations.Mappers
{
    public class ContractMapping : Profile
    {
        public ContractMapping()
        {
            CreateMap<UserCreateDtoRequest, UserEntity>();
            CreateMap<UserDtoContext, UserEntity>();
            CreateMap<StatusDtoContext, StatusEntity>();
        }
    }
}
