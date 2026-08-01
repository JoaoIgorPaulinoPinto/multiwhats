using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class SystemParameterRepository : ISystemParameterRepository
{
    private readonly AppDbContext _context;

    public SystemParameterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SystemParameter>> GetAllAsync()
    {
        return await _context.SystemParameters
            .AsNoTracking()
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Key)
            .ToListAsync();
    }

    public async Task<SystemParameter?> GetByKeyAsync(string key)
    {
        return await _context.SystemParameters
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == key);
    }

    public async Task<SystemParameter> AddAsync(SystemParameter parameter)
    {
        _context.SystemParameters.Add(parameter);
        await _context.SaveChangesAsync();
        return parameter;
    }

    public async Task<SystemParameter> UpdateAsync(SystemParameter parameter)
    {
        _context.Entry(parameter).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return parameter;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Id == id);
        if (param is null) return false;
        _context.SystemParameters.Remove(param);
        await _context.SaveChangesAsync();
        return true;
    }
}
