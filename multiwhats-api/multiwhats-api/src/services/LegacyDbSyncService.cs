using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.services;

public interface ILegacyDbSyncService
{
    Task SyncMessageAsync(Message message);
    Task SyncContactAsync(Contact contact);
    Task SyncClientAsync(Client client);
    Task SyncOccurrenceAsync(Occurrence occurrence);
    Task SyncTaskAsync(ClientTask task);
    Task SyncChatAsync(Chat chat);
    Task SyncDeviceAsync(Device device);
}

public class LegacyDbSyncService : ILegacyDbSyncService
{
    private readonly HttpClient _http;
    private readonly ILogger<LegacyDbSyncService> _logger;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public LegacyDbSyncService(HttpClient http, IConfiguration configuration, ILogger<LegacyDbSyncService> logger)
    {
        _http = http;
        _logger = logger;
        _baseUrl = configuration["LegacyDb:BaseUrl"] ?? "http://localhost:3001";
    }

    public async Task SyncMessageAsync(Message message)
    {
        // TODO: O campo MediaUrl contém base64. No LegacyDB (MySQL 4.1),
        // salvamos apenas um trecho (255 chars). O arquivo completo deve ser
        // salvo por outra API de armazenamento de arquivos (a implementar).
        var payload = new
        {
            message_id = message.MessageId,
            from_jid = message.FromJid,
            to_jid = message.ToJid,
            phone_number = message.PhoneNumber,
            body = message.Body,
            direction = message.Direction.ToString(),
            type = message.Type.ToString(),
            timestamp = message.Timestamp,
            chat_id = message.ChatId,
            user_id = message.UserId,
            occurrence_id = message.OccurrenceId,
            notify_name = message.NotifyName,
            has_media = message.HasMedia,
            media_url = message.MediaUrl?.Length > 255
                ? message.MediaUrl.Substring(0, 255)
                : message.MediaUrl,
            media_mime_type = message.MediaMimeType,
            media_filename = message.MediaFilename,
            media_size = message.MediaSize,
            media_caption = message.MediaCaption,
            is_forwarded = message.IsForwarded,
            reply_to_id = message.ReplyToId
        };

        await PostAsync("messages", payload);
    }

    public async Task SyncContactAsync(Contact contact)
    {
        var payload = new
        {
            jid = contact.Jid,
            phone_number = contact.PhoneNumber,
            name = contact.Name,
            push_name = contact.PushName,
            is_blocked = contact.IsBlocked,
            is_group = contact.IsGroup
        };

        await PostAsync("contacts", payload);
    }

    public async Task SyncClientAsync(Client client)
    {
        var payload = new
        {
            name = client.Name,
            main_phone_number = client.MainPhoneNumber,
            status = client.Status.ToString()
        };

        await PostAsync("clients", payload);
    }

    public async Task SyncOccurrenceAsync(Occurrence occurrence)
    {
        var payload = new
        {
            title = occurrence.Title,
            description = occurrence.Description,
            status = occurrence.Status.ToString(),
            priority = occurrence.Priority.ToString(),
            chat_id = occurrence.ChatId
        };

        await PostAsync("occurrences", payload);
    }

    public async Task SyncTaskAsync(ClientTask task)
    {
        var payload = new
        {
            title = task.Title,
            description = task.Description,
            status = task.Status.ToString(),
            priority = task.Priority.ToString(),
            due_date = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
            client_id = task.ClientId
        };

        await PostAsync("tasks", payload);
    }

    public async Task SyncChatAsync(Chat chat)
    {
        var payload = new
        {
            jid = chat.Jid,
            phone_number = chat.PhoneNumber,
            name = chat.Name
        };

        await PostAsync("chats", payload);
    }

    public async Task SyncDeviceAsync(Device device)
    {
        var payload = new
        {
            jid = device.Jid,
            phone_number = device.PhoneNumber,
            push_name = device.PushName,
            platform = device.Platform
        };

        await PostAsync("devices", payload);
    }

    private async Task PostAsync(string endpoint, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_baseUrl}/api/{endpoint}", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "LegacyDB sync falhou para {Endpoint}: {StatusCode} - {Body}",
                    endpoint, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar com LegacyDB: {Endpoint}", endpoint);
        }
    }
}
