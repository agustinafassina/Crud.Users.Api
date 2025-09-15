using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface ISessionsService
    {
        Task CreateSessionAsync(string userId, string sessionData);
    }
}