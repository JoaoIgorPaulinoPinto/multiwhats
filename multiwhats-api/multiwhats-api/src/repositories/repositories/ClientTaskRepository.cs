using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class ClientTaskRepository : IClientTaskRepository
{
    private readonly AppDbContext _context;

    public ClientTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientTask>> GetAllAsync()
    {
        return await _context.ClientTasks
            .AsNoTracking()
            .Include(t => t.Client)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<ClientTask?> GetByIdAsync(int id)
    {
        return await _context.ClientTasks
            .AsNoTracking()
            .Include(t => t.Client)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<ClientTask> AddAsync(ClientTask task)
    {
        _context.ClientTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<ClientTask> UpdateAsync(ClientTask task)
    {
        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.ClientTasks.FindAsync(id);
        if (task is not null)
        {
            _context.ClientTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<ClientTask>> GetByClientAsync(int clientId)
    {
        return await _context.ClientTasks
            .AsNoTracking()
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ClientTask>> GetByUserAsync(int userId)
    {
        return await _context.ClientTasks
            .AsNoTracking()
            .Where(t => t.AssignedToUserId == userId || t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
