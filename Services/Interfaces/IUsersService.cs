using UsersApi.ClientsDB.SqlServer.Dto;
using UsersApi.Mappers.Requests;
using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface IUsersService
    {
        Task<UserEntity> CreateUserAsync(UserEntity entity);
        Task<List<UserEntity>> GetUsers();
    }
}