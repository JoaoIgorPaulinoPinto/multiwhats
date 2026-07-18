using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;

    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Contact>> GetAllAsync()
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Group)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Group)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contact?> GetByJidAsync(string jid)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Jid == jid);
    }

    public async Task<Contact?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        _context.Entry(contact).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact is not null)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<Contact>> GetByClientAsync(int clientId)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<List<Contact>> GetByGroupAsync(int groupId)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => c.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesAsync(int contactId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ContactId == contactId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Occurrence>> GetOccurrencesAsync(int contactId)
    {
        return await _context.Occurrences
            .AsNoTracking()
            .Where(o => o.ContactId == contactId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
