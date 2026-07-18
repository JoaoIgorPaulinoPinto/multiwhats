using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class GetMessagesUseCase : IGetMessagesUseCase
{
    private readonly IMessageRepository _messageRepository;

    public GetMessagesUseCase(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<List<MessageResponse>> ExecuteAll()
    {
        var messages = await _messageRepository.GetAllAsync();
        return messages.Select(MapToResponse).ToList();
    }

    public async Task<MessageResponse?> ExecuteById(int id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        return message != null ? MapToResponse(message) : null;
    }

    public async Task<List<MessageResponse>> ExecuteByContact(int contactId)
    {
        var messages = await _messageRepository.GetByContactAsync(contactId);
        return messages.Select(MapToResponse).ToList();
    }

    public async Task<List<MessageResponse>> ExecuteByPhoneNumber(string phoneNumber)
    {
        var messages = await _messageRepository.GetByPhoneNumberAsync(PhoneNumberHelper.Sanitize(phoneNumber));
        return messages.Select(MapToResponse).ToList();
    }

    internal static MessageResponse MapToResponse(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            MessageId = message.MessageId,
            FromJid = message.FromJid,
            ToJid = message.ToJid,
            PhoneNumber = message.PhoneNumber,
            Body = message.Body,
            Direction = message.Direction,
            Type = message.Type,
            Timestamp = message.Timestamp,
            SentAt = message.SentAt,
            NotifyName = message.NotifyName,
            HasMedia = message.HasMedia,
            MediaUrl = message.MediaUrl,
            MediaMimeType = message.MediaMimeType,
            MediaFilename = message.MediaFilename,
            MediaSize = message.MediaSize,
            MediaCaption = message.MediaCaption,
            DeliveryStatus = message.DeliveryStatus,
            IsForwarded = message.IsForwarded,
            ContactId = message.ContactId,
            UserId = message.UserId,
            OccurrenceId = message.OccurrenceId,
            ReplyToId = message.ReplyToId,
            CreatedAt = message.CreatedAt,
        };
    }
}
