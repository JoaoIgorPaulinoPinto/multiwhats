using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

/// <summary>
/// USE CASE DE CRIAÇÃO DE CONTATO.
/// 
/// FLUXO:
/// 1. Verifica se já existe um contato com o mesmo JID (evita duplicatas)
/// 2. Cria a entidade Contact com os dados informados
/// 3. Salva no banco MySQL
/// 4. Verifica se já existe um Chat com esse JID → se sim, vincula ao contato
/// 5. Registra na auditoria
/// 6. Retorna os dados do contato criado
/// 
/// PECULIARIDADE: VINCULAÇÃO AUTOMÁTICA COM CHAT
/// - Quando um novo contato é criado, o sistema verifica se já existe um Chat
///   com o mesmo JID
/// - Se existir, vincula automaticamente o Chat ao Contact
/// - Isso acontece porque o Chat pode ser criado PRIMEIRO (quando chega uma mensagem)
///   e o Contact é criado DEPOIS (quando o operador cadastra)
/// 
/// O QUE É JID:
/// - JID = WhatsApp ID (ex: "5511999999999@c.us")
/// - É o identificador único de cada entidade no WhatsApp
/// - Contacts e Chats são vinculados pelo JID
/// 
/// SANITIZAÇÃO DE TELEFONE:
/// - O PhoneNumberHelper.Sanitize() remove caracteres não-numéricos
/// - Exemplo: "(11) 99999-9999" → "11999999999"
/// - Isso garante consistência no banco de dados
/// </summary>
public class CreateContactUseCase : ICreateContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateContactUseCase(
        IContactRepository contactRepository,
        IChatRepository chatRepository,
        UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
    }

    /// <summary>
    /// Executa a criação de um contato.
    /// 
    /// PASSOS DETALHADOS:
    /// 1. Validação: JID único (se já existe, lança exceção)
    /// 2. Criação: entidade Contact com dados sanitizados
    /// 3. Persistência: salva no MySQL
    /// 4. Vinculação: se existe Chat com mesmo JID, vincula
    /// 5. Auditoria: registra a ação
    /// </summary>
    public async Task<ContactDetailResponse> Execute(CreateContactRequest request, int userId)
    {
        // PASSO 1: Verificar se já existe um contato com esse JID
        var existing = await _contactRepository.GetByJidAsync(request.Jid);
        if (existing != null)
            throw new InvalidOperationException("Já existe um contato com este JID.");

        // PASSO 2: Criar a entidade Contact
        // PhoneNumberHelper.Sanitize() remove caracteres especiais do telefone
        var contact = new Contact(
            request.Jid,
            PhoneNumberHelper.Sanitize(request.PhoneNumber),  // Ex: "(11) 99999-9999" → "11999999999"
            request.Name,
            request.PushName,
            userId,
            request.ClientId,
            request.GroupId
        );

        // PASSO 3: Salvar no banco
        var created = await _contactRepository.AddAsync(contact);

        // PASSO 4: Vinculação automática com Chat
        // Se já existe um Chat com esse JID (criado quando uma mensagem chegou),
        // vincula o Chat ao Contact recém-criado
        var existingChat = await _chatRepository.GetByJidAsync(request.Jid);
        if (existingChat != null)
        {
            existingChat.LinkToContact(created.Id, created.ClientId);
            await _chatRepository.UpdateAsync(existingChat);
        }

        // PASSO 5: Registrar auditoria
        await _useCaseLogger.LogAsync(
            action: "CreateContact",
            entityType: "Contact",
            entityId: created.Id,
            description: $"Created contact \"{created.Name}\" (Jid: {created.Jid}, ClientId: {created.ClientId})",
            explicitUserId: userId
        );

        return MapToDetailResponse(created);
    }

    // ═══════════════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES DE MAPEAMENTO (Entity → Response DTO)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converte uma entidade Contact para ContactListResponse (resumo para listas).
    /// Usado quando retorna múltiplos contatos (GET /api/contacts).
    /// </summary>
    internal static ContactListResponse MapToListResponse(Contact contact)
    {
        return new ContactListResponse
        {
            Id = contact.Id,
            Name = contact.Name,
            PhoneNumber = contact.PhoneNumber,
            PushName = contact.PushName,
            IsBlocked = contact.IsBlocked,
            IsGroup = contact.IsGroup,
            ClientName = contact.Client?.Name,
            CreatedAt = contact.CreatedAt
        };
    }

    /// <summary>
    /// Converte uma entidade Contact para ContactDetailResponse (detalhes completos).
    /// Usado quando retorna um único contato (GET /api/contacts/{id}).
    /// </summary>
    internal static ContactDetailResponse MapToDetailResponse(Contact contact)
    {
        return new ContactDetailResponse
        {
            Id = contact.Id,
            Jid = contact.Jid,
            PhoneNumber = contact.PhoneNumber,
            Name = contact.Name,
            PushName = contact.PushName,
            ProfilePicUrl = contact.ProfilePicUrl,
            IsBlocked = contact.IsBlocked,
            IsGroup = contact.IsGroup,
            LastMessageAt = contact.LastMessageAt,
            ClientId = contact.ClientId,
            ClientName = contact.Client?.Name,
            GroupId = contact.GroupId,
            GroupName = contact.Group?.Name,
            CreatedByUserId = contact.CreatedByUserId,
            CreatedAt = contact.CreatedAt,
            LastUpdate = contact.LastUpdate
        };
    }
}
