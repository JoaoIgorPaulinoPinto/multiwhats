using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.repositories.interfaces;

public interface IMessageRepository
{
    Task<List<Message>> GetAllAsync();
    Task<Message?> GetByIdAsync(int id);
    Task<Message> AddAsync(Message message);
    Task<bool> DeleteAsync(int id);
    Task<List<Message>> GetByChatAsync(int chatId, int page, int pageSize);
    Task<int> GetByChatTotalCountAsync(int chatId);
    Task<List<Message>> GetByOccurrenceAsync(int occurrenceId);
    Task<List<Message>> GetByPhoneNumberAsync(string phoneNumber);
    Task<Message?> GetByMessageIdAsync(string messageId);
    Task<bool> MarkAsSystemAsync(string messageId);
    Task<Message?> UpdateDeliveryStatusAsync(string messageId, DeliveryStatus status);
    Task<List<Message>> MarkChatIncomingAsReadAsync(int chatId);
}
