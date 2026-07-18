using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class SendMessageUseCase : ISendMessageUseCase
{
    private readonly HttpClient _httpClient;
    private readonly IMessageRepository _messageRepository;
    private readonly IContactRepository _contactRepository;

    public SendMessageUseCase(
        HttpClient httpClient,
        IMessageRepository messageRepository,
        IContactRepository contactRepository)
    {
        _httpClient = httpClient;
        _messageRepository = messageRepository;
        _contactRepository = contactRepository;
    }

    public async Task<bool> Execute(SendMessageRequest request, int userId)
    {
        try
        {
            var phoneNumber = PhoneNumberHelper.Sanitize(request.PhoneNumber);

            var payloadNode = new
            {
                numero = phoneNumber,
                mensagem = request.Text
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloadNode),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("http://localhost:3000/api/enviar", jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ASP.NET] Resposta do Node.js -> Status: {response.StatusCode} | Corpo: {responseBody}");

            if (!response.IsSuccessStatusCode)
                return false;

            var raw = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var messageId = raw.TryGetProperty("messageId", out var mid) ? mid.GetString() : null;

            var contact = await _contactRepository.GetByPhoneNumberAsync(phoneNumber);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var message = new Message(
                fromJid: phoneNumber + "@c.us",
                phoneNumber: phoneNumber,
                body: request.Text,
                direction: MessageDirection.Outgoing,
                type: MessageType.Text,
                timestamp: timestamp,
                contactId: contact?.Id,
                userId: userId,
                messageId: messageId
            );

            await _messageRepository.AddAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao integrar com a API do WhatsApp: {ex.Message}");
            return false;
        }
    }
}
