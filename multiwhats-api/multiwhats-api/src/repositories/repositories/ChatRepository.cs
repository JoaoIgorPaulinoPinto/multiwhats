using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;

namespace multiwhats_api.src.repositories.repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;
    private readonly ILegacyDbSyncService _legacyDb;
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(AppDbContext context, ILegacyDbSyncService legacyDb, ILogger<ChatRepository> logger)
    {
        _context = context;
        _legacyDb = legacyDb;
        _logger = logger;
    }

    public async Task<List<Chat>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
                .ThenInclude(c => c.Client)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .Include(c => c.LastMessage)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Chats.CountAsync();
    }

    public async Task<Chat?> GetByIdAsync(int id)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
                .ThenInclude(c => c.Client)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .Include(c => c.LastMessage)
            .Include(c => c.Occurrences)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Chat?> GetByJidAsync(string jid)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
                .ThenInclude(c => c.Client)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Jid == jid);
    }

    public async Task<Chat?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
    }

    public async Task<Chat> AddAsync(Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try { await _legacyDb.SyncChatAsync(chat); }
            catch (Exception ex) { _logger.LogError(ex, "Erro ao sincronizar chat com LegacyDB"); }
        });

        return chat;
    }

    public async Task<Chat> UpdateAsync(Chat chat)
    {
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == id);
        if (chat is not null)
        {
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<int> GetMessageCountAsync(int chatId)
    {
        return await _context.Messages.CountAsync(m => m.ChatId == chatId);
    }

    public async Task<int> GetOccurrenceCountAsync(int chatId)
    {
        return await _context.Occurrences.CountAsync(o => o.ChatId == chatId);
    }

    public async Task<bool> MergeChatAsync(int sourceId, int destinationId)
    {
        if (sourceId == destinationId)
            return true;

        var source = await _context.Chats
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.Id == sourceId);

        var destination = await _context.Chats
            .FirstOrDefaultAsync(c => c.Id == destinationId);

        if (source is null || destination is null)
            return false;

        var messages = await _context.Messages
            .Where(m => m.ChatId == sourceId)
            .ToListAsync();

        foreach (var msg in messages)
            msg.UpdateChatId(destinationId);

        var occurrences = await _context.Occurrences
            .Where(o => o.ChatId == sourceId)
            .ToListAsync();

        foreach (var occ in occurrences)
            occ.UpdateChatId(destinationId);

        var latestMovedMessage = messages
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        if (latestMovedMessage is not null
            && (destination.LastMessageAt is null || latestMovedMessage.SentAt > destination.LastMessageAt.Value))
        {
            destination.UpdateLastMessage(latestMovedMessage.SentAt, latestMovedMessage);
        }

        if (source.Contact is not null && destination.ContactId is null)
        {
            destination.LinkToContact(source.Contact.Id, destination.ClientId ?? source.ClientId);
        }

        // Preserva a foto de perfil: se o destino não tem, aproveita a do
        // chat de origem (que pode ter sido capturada via getProfilePicUrl).
        if (destination.ProfilePicUrl is null && source.ProfilePicUrl is not null)
        {
            destination.UpdateProfilePicUrl(source.ProfilePicUrl);
        }

        _context.Chats.Remove(source);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Chat?> AssignUserAsync(int chatId, int userId)
    {
        var chat = await _context.Chats
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null) return null;

        chat.AssignUser(userId);

        await _context.ChatHistory.AddAsync( new ChatHistory (chatId, userId));
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task<Chat?> UnAssignUserAsync(int chatId, int userId)
    {
        var chat = await _context.Chats
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null || chat.AssignedToUserId != userId) return null;

        chat.UnassignUser();

        var openHistory = await _context.ChatHistory
            .Where(h => h.ChatId == chatId && h.UnassignedAt == null)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync();

        if (openHistory != null)
            openHistory.Finalize();

        await _context.SaveChangesAsync();
        return chat;
    }
}
