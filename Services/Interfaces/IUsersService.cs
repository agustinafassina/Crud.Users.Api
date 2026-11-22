using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface IUsersService
    {
        Task<UserEntity> CreateUserAsync(UserEntity entity);
        Task<string> GenerateJwtTokenAsync(UserLoginEntity user);
        Task<List<UserEntity>> GetUsers();
        Task<UserEntity?> ValidateUserAsync(string email, string password);
    }
}
