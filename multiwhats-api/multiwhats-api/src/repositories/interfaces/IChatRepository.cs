using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IChatRepository
{
    Task<List<Chat>> GetAllAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<Chat?> GetByIdAsync(int id);
    Task<Chat?> GetByJidAsync(string jid);
    Task<Chat?> GetByPhoneNumberAsync(string phoneNumber);
    Task<Chat> AddAsync(Chat chat);
    Task<Chat> UpdateAsync(Chat chat);
    Task<bool> DeleteAsync(int id);
    Task<int> GetMessageCountAsync(int chatId);
    Task<int> GetOccurrenceCountAsync(int chatId);
}
