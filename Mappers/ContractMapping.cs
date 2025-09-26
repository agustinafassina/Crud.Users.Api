
using AutoMapper;
using UsersApi.Mappers.Requests;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;

namespace UsersApi.Mappers
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
