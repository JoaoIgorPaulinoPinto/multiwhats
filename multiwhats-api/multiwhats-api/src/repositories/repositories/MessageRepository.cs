using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;

namespace multiwhats_api.src.repositories.repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;
    private readonly ILegacyDbSyncService _legacyDb;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(AppDbContext context, ILegacyDbSyncService legacyDb, ILogger<MessageRepository> logger)
    {
        _context = context;
        _legacyDb = legacyDb;
        _logger = logger;
    }

    public async Task<List<Message>> GetAllAsync()
    {
        return await _context.Messages
            .AsNoTracking()
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.Chat)
            .Include(m => m.Occurrence)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try { await _legacyDb.SyncMessageAsync(message); }
            catch (Exception ex) { _logger.LogError(ex, "Erro ao sincronizar mensagem com LegacyDB"); }
        });

        return message;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message is not null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<Message>> GetByChatAsync(int chatId, int page, int pageSize)
    {
        var messages = await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        messages.Reverse();

        return messages;
    }
    public async Task<int> GetByChatTotalCountAsync(int chatId)
    {
        return await _context.Messages.CountAsync(m => m.ChatId == chatId);
    }

    public async Task<List<Message>> GetByOccurrenceAsync(int occurrenceId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.OccurrenceId == occurrenceId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Message>> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.PhoneNumber == phoneNumber)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<Message?> GetByMessageIdAsync(string messageId)
    {
        return await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.WhatssAppMessageId == messageId);
    }
}
