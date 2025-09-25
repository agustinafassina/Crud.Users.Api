using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface IUsersService
    {
        Task<UserEntity> CreateUserAsync(UserEntity entity);
        string GenerateJwtToken(UserEntity user);
        Task<List<UserEntity>> GetUsers();
        Task<UserEntity> ValidateUserAsync(string email, string password);
    }
}