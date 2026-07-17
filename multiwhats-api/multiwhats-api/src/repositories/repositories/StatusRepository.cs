using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class StatusRepository : IStatusRepository
{
    private readonly AppDbContext _context;

    public StatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Status>> GetAllAsync()
    {
        return await _context.Status.AsNoTracking().ToListAsync();
    }

    public async Task<Status?> GetByIdAsync(int id)
    {
        return await _context.Status.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Status> AddAsync(Status status)
    {
        _context.Status.Add(status);
        await _context.SaveChangesAsync();
        return status;
    }

    public async Task<Status> UpdateAsync(Status status)
    {
        _context.Entry(status).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return status;
    }

    public async Task DeleteAsync(int id)
    {
        var status = await _context.Status.FindAsync(id);
        if (status is not null)
        {
            _context.Status.Remove(status);
            await _context.SaveChangesAsync();
        }
    }
}
