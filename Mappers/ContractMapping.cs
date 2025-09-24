
using AutoMapper;
using UsersApi.ClientsDB.SqlServer;
using UsersApi.ClientsDB.SqlServer.Dto;
using UsersApi.Mappers.Requests;
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
