using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface ISessionsService
    {
        Task CreateSessionAsync(int userId, string expirationToken, string token);
    }
}