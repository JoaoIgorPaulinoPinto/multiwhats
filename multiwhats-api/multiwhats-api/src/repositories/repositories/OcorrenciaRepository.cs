using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class OcorrenciaRepository : IOcorrenciaRepository
{
    private readonly AppDbContext _context;

    public OcorrenciaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ocorrencia>> GetAllAsync()
    {
        return await _context.Ocorrencias
            .AsNoTracking()
            .Include(o => o.Status)
            .ToListAsync();
    }

    public async Task<Ocorrencia?> GetByIdAsync(int id)
    {
        return await _context.Ocorrencias
            .AsNoTracking()
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Ocorrencia> AddAsync(Ocorrencia ocorrencia)
    {
        _context.Ocorrencias.Add(ocorrencia);
        await _context.SaveChangesAsync();
        return ocorrencia;
    }

    public async Task<Ocorrencia> UpdateAsync(Ocorrencia ocorrencia)
    {
        _context.Entry(ocorrencia).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return ocorrencia;
    }

    public async Task DeleteAsync(int id)
    {
        var ocorrencia = await _context.Ocorrencias.FindAsync(id);
        if (ocorrencia is not null)
        {
            _context.Ocorrencias.Remove(ocorrencia);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Ocorrencia>> GetByStatusAsync(int statusId)
    {
        return await _context.Ocorrencias
            .AsNoTracking()
            .Include(o => o.Status)
            .Where(o => o.StatusId == statusId)
            .ToListAsync();
    }
}
