using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

/// <summary>
/// USE CASE RESPONSÁVEL POR SALVAR MENSAGENS RECEBIDAS DO WHATSAPP.
/// 
/// FLUXO COMPLETO (passo a passo):
/// 1. Recebe o payload do WebhookController (que veio do Node.js)
/// 2. Verifica se a mensagem já existe (deduplicação)
/// 3. Detecta se é uma mensagem "auto-enviada" (o operador enviou pelo WhatsApp Web)
/// 4. Cria ou encontra o Chat correspondente
/// 5. Cria ou vincula o Contact ao Chat
/// 6. Salva a mensagem no banco MySQL (direção: Incoming ou Outgoing)
/// 7. Atualiza o "LastMessage" do Chat
/// 8. Envia notificação em tempo real para o Frontend via SignalR
/// 
/// DIFERENÇA DO SendMessageUseCase:
/// - SendMessageUseCase: ENVIA mensagens (ASP.NET → Node.js → WhatsApp)
/// - SaveIncomingMessageUseCase: RECEBE mensagens (WhatsApp → Node.js → ASP.NET)
/// 
/// O QUE É O WEBHOOK:
/// - É um "ponto de escuta" que o Node.js chama quando uma mensagem chega do WhatsApp
/// - O Node.js envia um POST para http://localhost:5261/api/webhook/whatsapp
/// - O WebhookController recebe esse POST e chama este Use Case
/// 
/// DEDUPLICAÇÃO:
/// - Às vezes o WhatsApp envia a mesma mensagem mais de uma vez (problemas de rede, etc.)
/// - Para evitar duplicatas, verificamos se já existe uma mensagem com o mesmo MessageId
/// </summary>
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

    /// <summary>
    /// Executa o salvamento de uma mensagem recebida do WhatsApp.
    /// 
    /// PASSO A PASSO DETALHADO:
    /// 
    /// PASSO 1: DEDUPLICAÇÃO
    /// - Verifica se já existe uma mensagem com esse MessageId
    /// - Se existir, ignora (evita duplicatas causadas por problemas de rede)
    /// 
    /// PASSO 2: DETECÇÃO DE AUTO-ENVIO
    /// - Verifica se o "From" da mensagem é o mesmo JID do dispositivo conectado
    /// - Se for, significa que o operador enviou a mensagem pelo WhatsApp Web
    /// - Nesse caso, a direção é "Outgoing" (não "Incoming")
    /// 
    /// PASSO 3: CRIAR/ENCONTRAR O CHAT
    /// - Se não existe um chat com esse JID, cria um novo
    /// - Se existe mas não tem Contact vinculado, tenta vincular
    /// 
    /// PASSO 4: SALVAR A MENSAGEM
    /// - Cria a entidade Message com direção correta (Incoming ou Outgoing)
    /// - Salva no MySQL via Repository
    /// 
    /// PASSO 5: ATUALIZAR O CHAT
    /// - Atualiza "LastMessageAt" e "LastMessageBody" do chat
    /// 
    /// PASSO 6: NOTIFICAR O FRONTEND
    /// - Envia evento "MessageReceived" via SignalR para todos os clientes conectados
    /// </summary>
    public async Task<bool> Execute(WhatsAppWebhookDto payload)
    {
        // ── PASSO 1: DEDUPLICAÇÃO ──
        // Remove caracteres não-numéricos do telefone (ex: "(11) 99999-9999" → "11999999999")
        var phoneNumber = PhoneNumberHelper.Sanitize(payload.PhoneNumber);

        // Verifica se já existe uma mensagem com esse MessageId no banco
        // Se existir, é uma duplicata e devemos ignorar
        if (!string.IsNullOrEmpty(payload.MessageId))
        {
            var existing = await _messageRepository.GetByMessageIdAsync(payload.MessageId);
            if (existing != null)
            {
                Console.WriteLine($"[SaveIncomingMessage] Duplicata ignorada msgId={payload.MessageId} (já existe id={existing.Id})");
                return false;
            }
        }

        // ── PASSO 2: DETECÇÃO DE AUTO-ENVIO ──
        // O WhatsApp Web.js às vezes envia de volta mensagens que o próprio operador enviou
        // Precisamos detectar isso para saber se a direção é Incoming ou Outgoing
        //
        // COMO FUNCIONA:
        // - O operador envia uma mensagem pelo WhatsApp Web
        // - O WhatsApp Web.js captura essa mensagem e envia de volta para o webhook
        // - O "from" da mensagem será o JID do próprio dispositivo
        // - Nesse caso, a mensagem é "auto-enviada" → direção = Outgoing
        var device = await _deviceRepository.GetCurrentAsync();
        var deviceJid = device?.Jid;

        var isSelfSent = deviceJid != null &&
            string.Equals(payload.From, deviceJid, StringComparison.OrdinalIgnoreCase);

        // Se for auto-envio: from = device JID, to = null, direction = Outgoing
        // Se for mensagem de outra pessoa: from = pessoa, to = device JID, direction = Incoming
        var actualFromJid = isSelfSent ? deviceJid! : payload.From;
        var direction = isSelfSent ? MessageDirection.Outgoing : MessageDirection.Incoming;
        var actualToJid = isSelfSent ? null : deviceJid;

        // ── PASSO 3: CRIAR/ENCONTRAR O CHAT ──
        // Busca um chat existente com o JID do remetente
        var chat = await _chatRepository.GetByJidAsync(payload.From);

        if (chat == null && !isSelfSent)
        {
            // ╔══════════════════════════════════════════════════════════╗
            // ║ CASO 1: NÃO EXISTE CHAT → Cria um novo automaticamente   ║
            // ╚══════════════════════════════════════════════════════════╝
            var contact = await _contactRepository.GetByJidAsync(payload.From);

            chat = new Chat(
                payload.From,                           // JID do remetente
                phoneNumber,                            // Telefone limpo
                payload.NotifyName ?? contact?.Name,    // Nome do perfil ou nome salvo
                contactId: contact?.Id,                 // Vincula ao contato, se existir
                clientId: contact?.ClientId             // Vincula ao cliente do contato
            );

            chat = await _chatRepository.AddAsync(chat);
        }
        else if (chat != null && !isSelfSent)
        {
            // ╔══════════════════════════════════════════════════════════╗
            // ║ CASO 2: CHAT EXISTE MAS SEM CONTACT → Vincula contato  ║
            // ╚══════════════════════════════════════════════════════════╝
            // Se o chat já existe mas não tem um Contact vinculado,
            // verifica se existe um Contact com esse JID e vincula
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

        // Se não conseguiu criar/encontrar chat, ignora a mensagem
        if (chat == null)
        {
            Console.WriteLine($"[SaveIncomingMessage] Ignorando mensagem auto-enviada (From: {payload.From}) sem chat conhecido.");
            return false;
        }

        // Busca o usuário que é dono da conexão WhatsApp
        var user = await _userRepository.GetByIdAsync(payload.UserId);
        int? userId = user?.Id;

        // ── PASSO 4: PARSEAR O TIPO DA MENSAGEM ──
        // Converte a string do payload para o enum MessageType
        // Exemplo: "image" → MessageType.Image
        var messageType = (payload.MessageType?.ToLower()) switch
        {
            "image" => MessageType.Image,
            "audio" => MessageType.Audio,
            "video" => MessageType.Video,
            "document" => MessageType.Document,
            "sticker" => MessageType.Sticker,
            "contact" => MessageType.Contact,
            "location" => MessageType.Location,
            _ => MessageType.Text                    // Padrão: texto
        };

        var timestamp = payload.Timestamp;

        // ── PASSO 5: SALVAR A MENSAGEM NO BANCO ──
        var message = new Message(
            fromJid: actualFromJid,                   // Quem enviou (JID)
            toJid: actualToJid,                       // Quem recebeu (JID) - null se auto-envio
            phoneNumber: phoneNumber,                  // Telefone limpo
            body: payload.Body,                        // Texto da mensagem
            direction: direction,                      // Incoming ou Outgoing
            type: messageType,                         // Tipo (text, image, audio, etc.)
            timestamp: timestamp,                      // Timestamp Unix
            chatId: chat.Id,                           // ID do chat
            userId: userId,                            // ID do operador
            messageId: payload.MessageId,              // ID do WhatsApp (deduplicação)
            notifyName: isSelfSent ? null : payload.NotifyName, // Nome do remetente
            hasMedia: payload.HasMedia,                // Se tem mídia
            mediaUrl: payload.MediaUrl,                // Base64 da mídia
            mediaMimeType: payload.MediaMimeType,      // Tipo MIME
            mediaFilename: payload.MediaFilename,      // Nome do arquivo
            mediaSize: payload.MediaSize,              // Tamanho
            mediaCaption: payload.MediaCaption,        // Legenda
            isForwarded: payload.IsForwarded           // Se foi encaminhada
        );

        await _messageRepository.AddAsync(message);

        Console.WriteLine($"[SaveIncomingMessage] Salvo msgId={payload.MessageId} type={messageType} hasMedia={payload.HasMedia} mediaUrlLen={payload.MediaUrl?.Length ?? 0} chatId={chat.Id}");

        // ── PASSO 6: ATUALIZAR O CHAT ──
        var sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        chat.UpdateLastMessage(sentAt, payload.Body);
        await _chatRepository.UpdateAsync(chat);

        // ── PASSO 7: REGISTRAR AUDITORIA ──
        var userName = user?.Name;
        await _useCaseLogger.LogAsync(
            action: "SaveIncomingMessage",
            entityType: "Message",
            entityId: null,
            description: $"{(isSelfSent ? "Self-sent" : "Received")} message {(isSelfSent ? "to" : "from")} {payload.From}: \"{Truncate(payload.Body, 80)}\" (type: {payload.MessageType}, direction: {direction})",
            explicitUserId: userId,
            explicitUserName: userName
        );

        // ── PASSO 8: NOTIFICAR O FRONTEND VIA SIGNALR ──
        // Envia o evento "MessageReceived" para todos os clientes conectados
        // O Frontend ouve esse evento e atualiza a interface em tempo real
        //
        // EXCEÇÃO: Mensagens auto-enviadas (self-sent) NÃO são broadcastadas aqui.
        // O SendMessageUseCase já enviou o evento "MessageSent" quando o usuário
        // enviou a mensagem original. Se broadcastássemos aqui também, o Frontend
        // receberia a mesma mensagem duas vezes (duplicata).
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
