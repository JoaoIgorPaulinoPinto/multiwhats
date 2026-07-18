using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Group>> GetAllAsync()
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Members)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups
            .AsNoTracking()
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group> AddAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<Group> UpdateAsync(Group group)
    {
        _context.Entry(group).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group is not null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<Contact>> GetMembersAsync(int groupId)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => c.GroupId == groupId)
            .ToListAsync();
    }
}
