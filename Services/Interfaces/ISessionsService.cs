
namespace UsersApi.Services.Interfaces
{
    public interface ISessionsService
    {
        Task CreateSessionAsync(int userId, System.DateTime expirationToken, string token);
    }
}