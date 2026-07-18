using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class SaveIncomingMessageUseCase : ISaveIncomingMessageUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUserRepository _userRepository;

    public SaveIncomingMessageUseCase(
        IMessageRepository repository,
        IContactRepository contactRepository,
        IUserRepository userRepository)
    {
        _messageRepository = repository;
        _contactRepository = contactRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Execute(WhatsAppWebhookDto payload)
    {
        var contact = await _contactRepository.GetByPhoneNumberAsync(payload.PhoneNumber);

        if (contact == null)
        {
            var newContact = new Contact(
                payload.From,
                payload.PhoneNumber,
                payload.NotifyName,
                payload.NotifyName
            );
            contact = await _contactRepository.AddAsync(newContact);
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

        var message = new Message(
            fromJid: payload.From,
            phoneNumber: payload.PhoneNumber,
            body: payload.Body,
            direction: MessageDirection.Incoming,
            type: messageType,
            timestamp: payload.Timestamp,
            contactId: contact.Id,
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
        return true;
    }
}
