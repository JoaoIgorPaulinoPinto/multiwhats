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
                return false;
            }
        }

        // Self-sent detection: use FromMe flag from webhook payload (more reliable than JID comparison)
        var device = await _deviceRepository.GetCurrentAsync();
        var deviceJid = device?.Jid;

        var isSelfSent = payload.FromMe;
        var source = payload.Source?.ToLowerInvariant() switch
        {
            "system" => MessageSource.System,
            "phone" => MessageSource.Phone,
            "contact" => MessageSource.Contact,
            _ => isSelfSent ? MessageSource.Phone : MessageSource.Contact
        };

        var actualFromJid = isSelfSent ? (deviceJid ?? payload.From) : payload.From;
        var direction = isSelfSent ? MessageDirection.Outgoing : MessageDirection.Incoming;
        var actualToJid = isSelfSent ? payload.From : deviceJid;

        var chat = await _chatRepository.GetByJidAsync(payload.From);

        if (chat == null)
        {
            // Cria o chat tanto para mensagens recebidas quanto para auto-enviadas
            // (ex.: mensagem enviada pelo celular para um contato sem chat no banco).
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

        // Mídia não aceita (áudio sempre; demais tipos fora de Media:AllowedTypes):
        // em vez de ignorar silenciosamente, salva uma mensagem de texto avisando o tipo,
        // para que o frontend exiba o aviso em tempo real.
        var allowedMedia = await _config.GetListAsync("Media:AllowedTypes", new List<string> { "Image", "Audio", "Video", "Document", "Sticker" });
        if (IsMediaType(messageType) && !isSelfSent &&
            (!allowedMedia.Contains(messageType.ToString())))
        {
            var ignoredType = messageType.ToString().ToLowerInvariant();

            messageType = MessageType.Text;
            payload = payload with
            {
                Body = $"midia:{ignoredType}|ignorado",
                HasMedia = false,
                MediaUrl = null,
                MediaMimeType = null,
                MediaFilename = null,
                MediaSize = null,
                MediaCaption = null
            };

            await _useCaseLogger.LogAsync(
                action: "SaveIncomingMessage",
                entityType: "Message",
                entityId: null,
                description: $"Unsupported media {ignoredType} ignored from {payload.From} (msgId={payload.MessageId})",
                explicitUserId: userId,
                explicitUserName: user?.Name
            );
        }

        // Auto-reply outside business hours/days (message is still saved so agents see it later).
        // Pulada durante sincronização inicial.

        /// Removido mensagem automatica de fora de horario de atendimento;
        /*
                if (!payload.IsSync && !isSelfSent && await IsOutsideBusinessHoursAsync(timestamp))
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
        */
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
            isForwarded: payload.IsForwarded,
            source: source,
            fromMe: isSelfSent
        );

        // Mensagens antigas sincronizadas já foram entregues no WhatsApp.
        if (payload.IsSync && direction == MessageDirection.Outgoing)
        {
            message.UpdateDeliveryStatus(DeliveryStatus.Delivered);
        }

        await _messageRepository.AddAsync(message);

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        chat.UpdateLastMessage(sentAt, message);
        await _chatRepository.UpdateAsync(chat);

        // Broadcast de tempo real. O webhook é a fonte canônica: para toda
        // mensagem não-sync o broadcast acontece AQUI. O SendMessageUseCase
        // só broadcasta (MessageSent) quando ELE salva a mensagem; se o
        // webhook já a salvou, o dedup acima retorna antes e evita o
        // broadcast duplicado. Isso garante exatamente UM broadcast por
        // mensagem, inclusive para envios via API (antes, na corrida em que
        // o webhook ganhava do dedup do SendMessageUseCase, nenhum broadcast
        // acontecia e a mensagem não aparecia em tempo real).
        // O evento é escolhido pela direção: outgoing vira MessageSent,
        // incoming vira MessageReceived.
        if (!payload.IsSync)
        {
            var msgResponse = GetMessagesUseCase.MapToDetailResponse(message);
            var eventName = direction == MessageDirection.Outgoing
                ? "MessageSent"
                : "MessageReceived";
            await _hubContext.Clients.All.SendAsync(eventName, msgResponse);
        }

        var userName = user?.Name;
        await _useCaseLogger.LogAsync(
            action: "SaveIncomingMessage",
            entityType: "Message",
            entityId: null,
            description: $"{(isSelfSent ? "Self-sent" : "Received")} message {(isSelfSent ? "to" : "from")} {payload.From}: \"{Truncate(payload.Body, 80)}\" (type: {payload.MessageType}, direction: {direction}, source: {source}{(payload.IsSync ? ", sync: true" : "")})",
            explicitUserId: userId,
            explicitUserName: userName
        );

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
