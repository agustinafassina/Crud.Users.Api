
using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface ISessionsService
    {
        Task CreateSessionAsync(UserLoginEntity user, DateTime expirationToken, string token);
    }
}