using UsersApi.Services.Dto;

namespace UsersApi.Services.Interfaces
{
    public interface ISessionsService
    {
        IEnumerable<ItemDto> GetAllItems();
        ItemDto GetItemById(int id);
        ItemDto CreateItem(ItemCreateDto newItem);
        Task CreateSessionAsync(string userId, string sessionData);
    }
}