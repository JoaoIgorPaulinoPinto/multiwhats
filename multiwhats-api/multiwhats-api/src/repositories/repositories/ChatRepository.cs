using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Chat>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
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
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(1))
            .Include(c => c.Occurrences)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Chat?> GetByJidAsync(string jid)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
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
        var chat = await _context.Chats.FindAsync(id);
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
}
