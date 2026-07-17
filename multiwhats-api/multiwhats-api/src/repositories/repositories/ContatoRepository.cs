using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly AppDbContext _context;

    public ContatoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Contato>> GetAllAsync()
    {
        return await _context.Contatos
            .AsNoTracking()
            .Include(c => c.Grupo)
            .Include(c => c.OcorrenciaAtual)
            .ToListAsync();
    }

    public async Task<Contato?> GetByIdAsync(int id)
    {
        return await _context.Contatos
            .AsNoTracking()
            .Include(c => c.Grupo)
            .Include(c => c.OcorrenciaAtual)
            .Include(c => c.Mensagens)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contato> AddAsync(Contato contato)
    {
        _context.Contatos.Add(contato);
        await _context.SaveChangesAsync();
        return contato;
    }

    public async Task<Contato> UpdateAsync(Contato contato)
    {
        _context.Entry(contato).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return contato;
    }

    public async Task DeleteAsync(int id)
    {
        var contato = await _context.Contatos.FindAsync(id);
        if (contato is not null)
        {
            _context.Contatos.Remove(contato);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Contato>> GetByGrupoAsync(int grupoId)
    {
        return await _context.Contatos
            .AsNoTracking()
            .Where(c => c.GrupoId == grupoId)
            .ToListAsync();
    }

    public async Task<List<Mensagem>> GetMensagensAsync(int contatoId)
    {
        return await _context.Set<Mensagem>()
            .AsNoTracking()
            .Where(m => m.ContatoId == contatoId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
}
