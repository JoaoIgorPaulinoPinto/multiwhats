using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class SendMessageUseCase : ISendMessageUseCase
{
    private readonly HttpClient _httpClient;
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public SendMessageUseCase(
        HttpClient httpClient,
        IMessageRepository messageRepository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IDeviceRepository deviceRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _httpClient = httpClient;
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
        _contactRepository = contactRepository;
        _deviceRepository = deviceRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    public async Task<bool> Execute(SendMessageRequest request, int userId)
    {
        try
        {
            var payloadNode = new
            {
                jid = request.Jid,
                mensagem = request.Text
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloadNode),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("http://localhost:3333/api/enviar", jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ASP.NET] Resposta do Node.js -> Status: {response.StatusCode} | Corpo: {responseBody}");

            if (!response.IsSuccessStatusCode)
                return false;

            var raw = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var messageId = raw.TryGetProperty("messageId", out var mid) ? mid.GetString() : null;

            var chat = await _chatRepository.GetByJidAsync(request.Jid);
            if (chat == null)
            {
                var phoneNumber = request.Jid.Split('@')[0];
                var contact = await _contactRepository.GetByJidAsync(request.Jid);

                chat = new Chat(
                    request.Jid,
                    phoneNumber,
                    contact?.Name,
                    contactId: contact?.Id,
                    clientId: contact?.ClientId
                );

                chat = await _chatRepository.AddAsync(chat);
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var phoneNumberFromJid = request.Jid.Split('@')[0];

            var device = await _deviceRepository.GetCurrentAsync();
            var deviceJid = device?.Jid;

            var message = new Message(
                fromJid: deviceJid ?? request.Jid,
                toJid: request.Jid,
                phoneNumber: phoneNumberFromJid,
                body: request.Text,
                direction: MessageDirection.Outgoing,
                type: MessageType.Text,
                timestamp: timestamp,
                chatId: chat.Id,
                userId: userId,
                messageId: messageId
            );

            await _messageRepository.AddAsync(message);

            chat.UpdateLastMessage(DateTime.UtcNow, request.Text);
            await _chatRepository.UpdateAsync(chat);

            await _useCaseLogger.LogAsync(
                action: "SendMessage",
                entityType: "Message",
                entityId: null,
                description: $"Sent message to {request.Jid}: \"{Truncate(request.Text, 80)}\" (direction: Outgoing)",
                explicitUserId: userId
            );

            var msgResponse = GetMessagesUseCase.MapToSummaryResponse(message);
            await _hubContext.Clients.All.SendAsync("MessageSent", msgResponse);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao integrar com a API do WhatsApp: {ex.Message}");

            await _useCaseLogger.LogAsync(
                action: "SendMessage",
                entityType: "Message",
                entityId: null,
                description: $"Failed to send message to {request.Jid}: {ex.Message}",
                explicitUserId: userId
            );

            return false;
        }
    }

    private static string Truncate(string? value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
    }
}
