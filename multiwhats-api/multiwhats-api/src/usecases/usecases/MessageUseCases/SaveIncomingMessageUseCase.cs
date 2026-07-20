using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class SaveIncomingMessageUseCase : ISaveIncomingMessageUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUserRepository _userRepository;

    public SaveIncomingMessageUseCase(
        IMessageRepository repository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository)
    {
        _messageRepository = repository;
        _chatRepository = chatRepository;
        _contactRepository = contactRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Execute(WhatsAppWebhookDto payload)
    {
        var phoneNumber = PhoneNumberHelper.Sanitize(payload.PhoneNumber);

        var chat = await _chatRepository.GetByJidAsync(payload.From);
        if (chat == null)
        {
            var contact = await _contactRepository.GetByJidAsync(payload.From);

            chat = new Chat(
                payload.From,
                phoneNumber,
                payload.NotifyName ?? contact?.Name,
                contactId: contact?.Id,
                clientId: contact?.ClientId
            );

            chat = await _chatRepository.AddAsync(chat);
        }
        else
        {
            if (chat.ContactId == null)
            {
                var contact = await _contactRepository.GetByJidAsync(payload.From);
                if (contact != null)
                {
                    chat.LinkToContact(contact.Id, contact.ClientId);
                    await _chatRepository.UpdateAsync(chat);
                }
            }
        }

        var user = await _userRepository.GetByIdAsync(payload.UserId);
        int? userId = user?.Id;

        var messageType = (payload.MessageType?.ToLower()) switch
        {
            "image" => MessageType.Image,
            "audio" => MessageType.Audio,
            "video" => MessageType.Video,
            "document" => MessageType.Document,
            "sticker" => MessageType.Sticker,
            "contact" => MessageType.Contact,
            "location" => MessageType.Location,
            _ => MessageType.Text
        };

        var timestamp = payload.Timestamp;

        var message = new Message(
            fromJid: payload.From,
            phoneNumber: phoneNumber,
            body: payload.Body,
            direction: MessageDirection.Incoming,
            type: messageType,
            timestamp: timestamp,
            chatId: chat.Id,
            userId: userId,
            messageId: payload.MessageId,
            notifyName: payload.NotifyName,
            hasMedia: payload.HasMedia,
            mediaUrl: payload.MediaUrl,
            mediaMimeType: payload.MediaMimeType,
            mediaFilename: payload.MediaFilename,
            mediaSize: payload.MediaSize,
            isForwarded: payload.IsForwarded
        );

        await _messageRepository.AddAsync(message);

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        chat.UpdateLastMessage(sentAt, payload.Body);
        await _chatRepository.UpdateAsync(chat);

        return true;
    }
}
