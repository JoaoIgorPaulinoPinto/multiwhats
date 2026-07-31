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
    private readonly SystemConfigService _config;

    public SaveIncomingMessageUseCase(
        IMessageRepository repository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext,
        HttpClient httpClient,
        ISendMessageUseCase sendMessageUseCase,
        SystemConfigService config)
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
        _config = config;
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

        // Self-sent detection: use FromMe flag from webhook payload (more reliable than JID comparison)
        var device = await _deviceRepository.GetCurrentAsync();
        var deviceJid = device?.Jid;

        var isSelfSent = payload.FromMe;

        var actualFromJid = isSelfSent ? (deviceJid ?? payload.From) : payload.From;
        var direction = isSelfSent ? MessageDirection.Outgoing : MessageDirection.Incoming;
        var actualToJid = isSelfSent ? payload.From : deviceJid;

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

        // Block unsupported incoming media (audio always blocked; other media blocked when not in Media:AllowedTypes)
        var allowedMedia = await _config.GetListAsync("Media:AllowedTypes", new List<string> { "Image", "Audio", "Video", "Document", "Sticker" });
        if (IsMediaType(messageType) && !isSelfSent &&
            (messageType == MessageType.Audio || !allowedMedia.Contains(messageType.ToString())))
        {
            Console.WriteLine($"[SaveIncomingMessage] Mídia não suportada bloqueada de {payload.From} (msgId={payload.MessageId}, type={messageType})");

            if (userId != null)
            {
                try
                {
                    var unsupportedMsg = await _config.GetStringAsync(
                        "Media:UnsupportedMessage",
                        "Desculpe, não consigo processar este tipo de mídia. Por favor, envie apenas texto, imagens, vídeos ou documentos.");

                    var reply = new SendMessageRequest
                    {
                        Jid = payload.From,
                        Text = unsupportedMsg,
                        Type = MessageType.Text,
                        SenderName = await GetAutoReplySenderNameAsync()
                    };
                    await _sendMessageUseCase.Execute(reply, userId.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveIncomingMessage] Erro ao enviar resposta de mídia não suportada: {ex.Message}");
                }
            }

            await _useCaseLogger.LogAsync(
                action: "SaveIncomingMessage",
                entityType: "Message",
                entityId: null,
                description: $"Unsupported media blocked from {payload.From} (msgId={payload.MessageId}, type={messageType})",
                explicitUserId: userId,
                explicitUserName: user?.Name
            );

            return true;
        }

        // Auto-reply outside business hours/days (message is still saved so agents see it later)
        if (!isSelfSent && await IsOutsideBusinessHoursAsync(timestamp))
        {
            var outsideMsg = await BuildOutsideHoursMessageAsync();
            if (userId != null)
            {
                try
                {
                    var reply = new SendMessageRequest
                    {
                        Jid = payload.From,
                        Text = outsideMsg,
                        Type = MessageType.Text,
                        SenderName = await GetAutoReplySenderNameAsync()
                    };
                    await _sendMessageUseCase.Execute(reply, userId.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveIncomingMessage] Erro ao enviar resposta fora do horário: {ex.Message}");
                }
            }

            await _useCaseLogger.LogAsync(
                action: "SaveIncomingMessage",
                entityType: "Message",
                entityId: null,
                description: $"Outside business hours auto-reply sent to {payload.From} (msgId={payload.MessageId})",
                explicitUserId: userId,
                explicitUserName: user?.Name
            );
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

    private static bool IsMediaType(MessageType type)
    {
        return type is MessageType.Image or MessageType.Audio or MessageType.Video or MessageType.Document or MessageType.Sticker;
    }

    private async Task<string?> GetAutoReplySenderNameAsync()
    {
        var name = await _config.GetStringAsync("Replies:SenderName", "");
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private async Task<bool> IsOutsideBusinessHoursAsync(long unixTimestamp)
    {
        if (!await _config.GetBoolAsync("Business:Enabled", false))
            return false;

        var timezoneId = await _config.GetStringAsync("Business:Timezone", "America/Sao_Paulo");
        TimeZoneInfo tz;
        if (string.IsNullOrWhiteSpace(timezoneId))
        {
            tz = TimeZoneInfo.Utc;
        }
        else
        {
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.Utc;
            }
        }

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(
            DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime, tz);

        var workingDays = await _config.GetListAsync("Business:WorkingDays", new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" });
        if (!workingDays.Contains(localTime.DayOfWeek.ToString(), StringComparer.OrdinalIgnoreCase))
            return true;

        var open = await _config.GetStringAsync("Business:OpenTime", "08:00");
        var close = await _config.GetStringAsync("Business:CloseTime", "18:00");
        if (!TimeSpan.TryParse(open, out var openTs) || !TimeSpan.TryParse(close, out var closeTs))
            return false;

        var now = localTime.TimeOfDay;
        if (openTs <= closeTs)
            return now < openTs || now > closeTs;

        // Overnight shift (e.g. 22:00 - 06:00)
        return now < openTs && now > closeTs;
    }

    private async Task<string> BuildOutsideHoursMessageAsync()
    {
        var message = (await _config.GetStringAsync(
            "Business:OutsideHoursMessage",
            "Olá! Nosso atendimento funciona de {days} das {open} às {close}. Recebemos sua mensagem e retornaremos no próximo horário de funcionamento.")) ?? "";

        var open = (await _config.GetStringAsync("Business:OpenTime", "08:00")) ?? "";
        var close = (await _config.GetStringAsync("Business:CloseTime", "18:00")) ?? "";
        var workingDays = await _config.GetListAsync("Business:WorkingDays", new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" });

        var ptDays = string.Join(", ", workingDays.Select(d => d.ToLowerInvariant() switch
        {
            "monday" => "segunda-feira",
            "tuesday" => "terça-feira",
            "wednesday" => "quarta-feira",
            "thursday" => "quinta-feira",
            "friday" => "sexta-feira",
            "saturday" => "sábado",
            "sunday" => "domingo",
            _ => d
        }));

        return message
            .Replace("{open}", open)
            .Replace("{close}", close)
            .Replace("{days}", ptDays);
    }
}
