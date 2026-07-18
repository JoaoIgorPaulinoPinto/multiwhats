using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class OccurrenceRepository : IOccurrenceRepository
{
    private readonly AppDbContext _context;

    public OccurrenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Occurrence>> GetAllAsync()
    {
        return await _context.Occurrences
            .AsNoTracking()
            .Include(o => o.Contact)
            .Include(o => o.AssignedTo)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Occurrence?> GetByIdAsync(int id)
    {
        return await _context.Occurrences
            .AsNoTracking()
            .Include(o => o.Contact)
            .Include(o => o.AssignedTo)
            .Include(o => o.CreatedBy)
            .Include(o => o.Messages)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Occurrence> AddAsync(Occurrence occurrence)
    {
        _context.Occurrences.Add(occurrence);
        await _context.SaveChangesAsync();
        return occurrence;
    }

    public async Task<Occurrence> UpdateAsync(Occurrence occurrence)
    {
        _context.Entry(occurrence).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return occurrence;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var occurrence = await _context.Occurrences.FindAsync(id);
        if (occurrence is not null)
        {
            _context.Occurrences.Remove(occurrence);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<Occurrence>> GetByContactAsync(int contactId)
    {
        return await _context.Occurrences
            .AsNoTracking()
            .Where(o => o.ContactId == contactId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Occurrence>> GetByUserAsync(int userId)
    {
        return await _context.Occurrences
            .AsNoTracking()
            .Where(o => o.AssignedToUserId == userId || o.CreatedByUserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
