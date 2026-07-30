using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using multiwhats_api.src.data.dtos.Requests;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

// Saves incoming webhook messages from WhatsApp, handles dedup, self-sent detection, and auto-creates chats.
public class SaveIncomingMessageUseCase : ISaveIncomingMessageUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;
    private readonly HttpClient _httpClient;
    private readonly ISendMessageUseCase _sendMessageUseCase;

    public SaveIncomingMessageUseCase(
        IMessageRepository repository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext,
        HttpClient httpClient,
        ISendMessageUseCase sendMessageUseCase)
    {
        _messageRepository = repository;
        _chatRepository = chatRepository;
        _contactRepository = contactRepository;
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
        _httpClient = httpClient;
        _sendMessageUseCase = sendMessageUseCase;
    }


    public async Task<bool> Execute(WhatsAppWebhookDto payload)
    {
        var phoneNumber = PhoneNumberHelper.Sanitize(payload.PhoneNumber);

        // Deduplication: ignore messages already processed
        if (!string.IsNullOrEmpty(payload.MessageId))
        {
            var existing = await _messageRepository.GetByMessageIdAsync(payload.MessageId);
            if (existing != null)
            {
                Console.WriteLine($"[SaveIncomingMessage] Duplicata ignorada msgId={payload.MessageId} (já existe id={existing.Id})");
                return false;
            }
        }

        // Self-sent detection: messages from our own device are marked as Outgoing
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

        var messageType = payload.MessageType?.ToLowerInvariant() switch
        {
            "image" => MessageType.Image,
            "audio" or "ptt" => MessageType.Audio,
            "video" => MessageType.Video,
            "document" => MessageType.Document,
            "sticker" => MessageType.Sticker,
            "vcard" or "contact_card" or "multi_vcard" => MessageType.Contact,
            "location" or "live_location" => MessageType.Location,
            _ => MessageType.Text
        };

        var timestamp = payload.Timestamp;

        // Block incoming audio messages
        if (messageType == MessageType.Audio && !isSelfSent)
        {
            Console.WriteLine($"[SaveIncomingMessage] Áudio bloqueado de {payload.From} (msgId={payload.MessageId})");

            if (userId != null)
            {
                try
                {
                    var audioBlockedMsg = new SendMessageRequest
                    {
                        Jid = payload.From,
                        Text = "Desculpe, não podemos receber áudio. Por gentileza, digite.",
                        Type = MessageType.Text
                    };
                    await _sendMessageUseCase.Execute(audioBlockedMsg, userId.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveIncomingMessage] Erro ao enviar resposta de bloqueio de áudio: {ex.Message}");
                }
            }

            await _useCaseLogger.LogAsync(
                action: "SaveIncomingMessage",
                entityType: "Message",
                entityId: null,
                description: $"Audio blocked from {payload.From} (msgId={payload.MessageId})",
                explicitUserId: userId,
                explicitUserName: user?.Name
            );

            return true;
        }

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
            mediaCaption: payload.MediaCaption,
            isForwarded: payload.IsForwarded
        );
        Console.WriteLine(messageType);
        await _messageRepository.AddAsync(message);

        Console.WriteLine($"[SaveIncomingMessage] Salvo msgId={payload.MessageId} type={messageType} hasMedia={payload.HasMedia} mediaUrlLen={payload.MediaUrl?.Length ?? 0} chatId={chat.Id}");

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        chat.UpdateLastMessage(sentAt, message);
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

        // Self-sent messages already broadcast by SendMessageUseCase, skip to avoid duplicates
        if (!isSelfSent)
        {
            var msgResponse = GetMessagesUseCase.MapToDetailResponse(message);
            await _hubContext.Clients.All.SendAsync("MessageReceived", msgResponse);
        }


        return true;
    }

    private static string Truncate(string? value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
    }
}
