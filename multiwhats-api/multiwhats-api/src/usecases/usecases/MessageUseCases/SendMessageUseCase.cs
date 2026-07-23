using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.strategies;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

/// <summary>
/// USE CASE RESPONSÁVEL POR ENVIAR MENSAGENS PELO WHATSAPP.
/// 
/// FLUXO COMPLETO (passo a passo):
/// 1. Recebe a requisição do controller (JID, texto, tipo, mídia)
/// 2. Usa o Strategy Pattern para escolher como tratar o tipo de mensagem
/// 3. Envia a mensagem para o Node.js (serviço de messageria)
/// 4. Salva a mensagem no banco MySQL (direção: Outgoing)
/// 5. Atualiza o "LastMessage" do Chat
/// 6. Envia notificação em tempo real para o Frontend via SignalR
/// 
/// O QUE É O NODE.JS:
/// - É um serviço separado que fala diretamente com o WhatsApp Web
/// - O ASP.NET não consegue falar com o WhatsApp sozinho, então ele
///   pede pro Node.js fazer isso via HTTP POST para localhost:3333
/// 
/// O QUE É O JID:
/// - JID = WhatsApp ID. É o identificador único de cada contato no WhatsApp
/// - Formato: "5511999999999@c.us" (número@servidor)
/// - É usado para saber PARA QUEM a mensagem está sendo enviada
/// </summary>
public class SendMessageUseCase : ISendMessageUseCase
{
    // ═══════════════════════════════════════════════════════════════════
    // DEPENDÊNCIAS (injetadas pelo construtor via DI)
    // ═══════════════════════════════════════════════════════════════════

    private readonly HttpClient _httpClient;                    // HTTP client para chamar o Node.js (localhost:3333)
    private readonly IMessageRepository _messageRepository;    // Salvar mensagens no banco
    private readonly IChatRepository _chatRepository;          // Gerenciar conversas
    private readonly IContactRepository _contactRepository;    // Buscar contatos existentes
    private readonly IDeviceRepository _deviceRepository;      // Saber qual dispositivo está conectado
    private readonly MessageStrategyFactory _strategyFactory;  // Factory do Strategy Pattern (escolhe tipo de msg)
    private readonly UseCaseLogger _useCaseLogger;             // Logger de auditoria
    private readonly IHubContext<WhatsappHub> _hubContext;     // SignalR para notificar o Frontend em tempo real

    public SendMessageUseCase(
        HttpClient httpClient,
        IMessageRepository messageRepository,
        IChatRepository chatRepository,
        IContactRepository contactRepository,
        IDeviceRepository deviceRepository,
        MessageStrategyFactory strategyFactory,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _httpClient = httpClient;
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
        _contactRepository = contactRepository;
        _deviceRepository = deviceRepository;
        _strategyFactory = strategyFactory;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Executa o envio de uma mensagem WhatsApp.
    /// 
    /// PASSO A PASSO DETALHADO:
    /// 
    /// PASSO 1: STRATEGY PATTERN
    /// - O factory escolhe a strategy correta (Text, Image, Audio, etc.)
    /// - Cada strategy sabe como: (a) montar o payload pro Node.js e (b) extrair campos pro banco
    /// 
    /// PASSO 2: ENVIAR PARA O NODE.JS
    /// - POST http://localhost:3333/api/enviar com o payload JSON
    /// - O Node.js usa WhatsApp Web.js (Puppeteer) para enviar a mensagem de verdade
    /// - Retorna o "messageId" do WhatsApp (ID único para deduplicação)
    /// 
    /// PASSO 3: CRIAR/ENCONTRAR O CHAT
    /// - Se já existe um chat com esse JID, usa ele
    /// - Se não existe, cria um novo automaticamente
    /// - Se já existe um contato com esse JID, vincula ao chat
    /// 
    /// PASSO 4: SALVAR NO BANCO
    /// - Cria a entidade Message com direção "Outgoing"
    /// - Salva no MySQL via Repository
    /// 
    /// PASSO 5: ATUALIZAR O CHAT
    /// - Atualiza "LastMessageAt" e "LastMessageBody" do chat
    /// 
    /// PASSO 6: NOTIFICAR O FRONTEND
    /// - Envia evento "MessageSent" via SignalR para todos os clientes conectados
    /// 
    /// PASSO 7: REGISTRAR AUDITORIA
    /// - Salva log dizendo quem enviou, para quem, e o que
    /// </summary>
    public async Task<bool> Execute(SendMessageRequest request, int userId)
    {
        try
        {
            // ── PASSO 1: STRATEGY PATTERN ──
            // Obtém a strategy correta para o tipo de mensagem (text, image, audio, etc.)
            // Exemplo: para "text", usa TextMessageStrategy; para "image", usa ImageMessageStrategy
            var strategy = _strategyFactory.Get(request.Type);

            // Monta o payload JSON que o Node.js espera receber
            // Exemplo para texto: { "jid": "5511999999999@c.us", "mensagem": "Olá", "type": "text" }
            var payloadNode = strategy.BuildNodePayload(request.Jid, request);

            // Serializa o payload para JSON e prepara para enviar via HTTP
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloadNode),
                Encoding.UTF8,
                "application/json"
            );

            // ── PASSO 2: ENVIAR PARA O NODE.JS ──
            // O Node.js está rodando em localhost:3333 e usa WhatsApp Web.js para enviar
            var response = await _httpClient.PostAsync("http://localhost:3333/api/enviar", jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ASP.NET] Resposta do Node.js -> Status: {response.StatusCode} | Corpo: {responseBody}");

            // Se o Node.js retornou erro (ex: WhatsApp desconectado), retorna false
            if (!response.IsSuccessStatusCode)
                return false;

            // Extrai o "messageId" da resposta do Node.js
            // Este ID é usado para deduplicação (evitar salvar a mesma mensagem duas vezes)
            var raw = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var messageId = raw.TryGetProperty("messageId", out var mid) ? mid.GetString() : null;

            // ── PASSO 3: CRIAR/ENCONTRAR O CHAT ──
            // Se já existe um chat com esse JID, usa ele. Se não, cria um novo.
            var chat = await _chatRepository.GetByJidAsync(request.Jid);
            if (chat == null)
            {
                // Extrai o phoneNumber do JID (remove o "@c.us" do final)
                // Exemplo: "5511999999999@c.us" → "5511999999999"
                var phoneNumber = request.Jid.Split('@')[0];

                // Verifica se já existe um contato com esse JID
                // Se existir, já vincula o chat ao contato (e ao cliente do contato)
                var contact = await _contactRepository.GetByJidAsync(request.Jid);

                chat = new Chat(
                    request.Jid,
                    phoneNumber,
                    contact?.Name,                   // Nome do contato, se existir
                    contactId: contact?.Id,          // Vincula ao contato, se existir
                    clientId: contact?.ClientId      // Vincula ao cliente do contato, se existir
                );

                chat = await _chatRepository.AddAsync(chat);
            }

            // ── PASSO 4: SALVAR A MENSAGEM NO BANCO ──
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Timestamp Unix (segundos desde 1970)
            var phoneNumberFromJid = request.Jid.Split('@')[0];

            // Busca o dispositivo conectado para saber o JID do remetente
            // Exemplo: se o WhatsApp está conectado como "5511888888888@c.us",
            //          o "fromJid" será esse JID (quem enviou a mensagem)
            var device = await _deviceRepository.GetCurrentAsync();
            var deviceJid = device?.Jid;

            // Usa a strategy para extrair os campos da mensagem (body, media, etc.)
            var fields = strategy.BuildMessageFields(request);

            // Cria a entidade Message com direção "Outgoing" (mensagem enviada por nós)
            var message = new Message(
                fromJid: deviceJid ?? request.Jid,    // JID do remetente (dispositivo conectado)
                toJid: request.Jid,                    // JID do destinatário
                phoneNumber: phoneNumberFromJid,       // Número de telefone limpo
                body: fields.body,                     // Texto da mensagem
                direction: MessageDirection.Outgoing,  // "Outgoing" = mensagem que enviamos
                type: strategy.Type,                   // Tipo (Text, Image, Audio, etc.)
                timestamp: timestamp,                  // Timestamp Unix
                chatId: chat.Id,                       // ID do chat (obrigatório)
                userId: userId,                        // ID do operador que enviou
                messageId: messageId,                  // ID do WhatsApp (deduplicação)
                hasMedia: fields.hasMedia,             // Se tem mídia (imagem, áudio, etc.)
                mediaUrl: fields.mediaUrl,             // Base64 da mídia
                mediaMimeType: fields.mediaMimeType,   // Tipo MIME (ex: "image/jpeg")
                mediaFilename: fields.mediaFilename,   // Nome do arquivo
                mediaSize: fields.mediaSize,           // Tamanho em bytes
                mediaCaption: fields.mediaCaption      // Legenda da mídia
            );
            Console.WriteLine(message.MediaUrl);

            await _messageRepository.AddAsync(message);

            // ── PASSO 5: ATUALIZAR O CHAT ──
            // Atualiza a data e o corpo da última mensagem do chat
            // Isso é usado no Frontend para mostrar "última mensagem: Olá..." na lista de chats
            chat.UpdateLastMessage(DateTime.UtcNow, fields.body);
            await _chatRepository.UpdateAsync(chat);

            // ── PASSO 6: REGISTRAR AUDITORIA ──
            await _useCaseLogger.LogAsync(
                action: "SendMessage",
                entityType: "Message",
                entityId: null,
                description: $"Sent {strategy.Type} message to {request.Jid}: \"{Truncate(fields.body, 80)}\" (direction: Outgoing)",
                explicitUserId: userId
            );

            // ── PASSO 7: NOTIFICAR O FRONTEND VIA SIGNALR ──
            // SignalR é como um "WebSocket gerenciado" que permite o servidor
            // enviar mensagens para o Frontend em tempo real (sem o Frontend precisar perguntar)
            // O evento "MessageSent" é ouvido pelo Frontend que atualiza a UI instantaneamente
            var msgResponse = GetMessagesUseCase.MapToSummaryResponse(message);
            await _hubContext.Clients.All.SendAsync("MessageSent", msgResponse);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao integrar com a API do WhatsApp: {ex.Message}");

            // Mesmo com erro, registra na auditoria (para debug)
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

    /// <summary>
    /// Trunca uma string para caber em logs (evita textos gigantes no console).
    /// Se "Olá, tudo bem? Como você está? Espero que esteja bem!" tiver mais de 80 chars,
    /// retorna apenas os primeiros 80 + "..."
    /// </summary>
    private static string Truncate(string? value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
    }
}
