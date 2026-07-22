using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class SaveIncomingMessageUseCase : ISaveIncomingMessageUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public SaveIncomingMessageUseCase(
        IMessageRepository repository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _messageRepository = repository;
        _chatRepository = chatRepository;
        _contactRepository = contactRepository;
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    public async Task<bool> Execute(WhatsAppWebhookDto payload)
    {
        var phoneNumber = PhoneNumberHelper.Sanitize(payload.PhoneNumber);

        var device = await _deviceRepository.GetCurrentAsync();
        var deviceJid = device?.Jid;

        var isSelfSent = deviceJid != null &&
            string.Equals(payload.From, deviceJid, StringComparison.OrdinalIgnoreCase);

        var actualFromJid = isSelfSent ? deviceJid! : payload.From;
        var direction = isSelfSent ? MessageDirection.Outgoing : MessageDirection.Incoming;
        var actualToJid = isSelfSent ? null : deviceJid;

        var chat = await _chatRepository.GetByJidAsync(payload.From);
        if (chat == null && !isSelfSent)
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
        else if (chat != null && !isSelfSent)
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

        if (chat == null)
        {
            Console.WriteLine($"[SaveIncomingMessage] Ignorando mensagem auto-enviada (From: {payload.From}) sem chat conhecido.");
            return false;
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
            fromJid: actualFromJid,
            toJid: actualToJid,
            phoneNumber: phoneNumber,
            body: payload.Body,
            direction: direction,
            type: messageType,
            timestamp: timestamp,
            chatId: chat.Id,
            userId: userId,
            messageId: payload.MessageId,
            notifyName: isSelfSent ? null : payload.NotifyName,
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

        var userName = user?.Name;
        await _useCaseLogger.LogAsync(
            action: "SaveIncomingMessage",
            entityType: "Message",
            entityId: null,
            description: $"{(isSelfSent ? "Self-sent" : "Received")} message {(isSelfSent ? "to" : "from")} {payload.From}: \"{Truncate(payload.Body, 80)}\" (type: {payload.MessageType}, direction: {direction})",
            explicitUserId: userId,
            explicitUserName: userName
        );

        var msgResponse = GetMessagesUseCase.MapToDetailResponse(message);
        await _hubContext.Clients.All.SendAsync("MessageReceived", msgResponse);

        return true;
    }

    private static string Truncate(string? value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
    }
}
