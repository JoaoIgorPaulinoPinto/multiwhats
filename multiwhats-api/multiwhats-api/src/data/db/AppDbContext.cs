using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.data.db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Mensagem> Mensagens { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<Ocorrencia> Ocorrencias { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Status> Status { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mensagem>(entity =>
        {
            entity.ToTable("mensagens");
        });
    }
    public override int SaveChanges()
    {
        ApplyTracking();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTracking();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTracking()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var trackable = (BaseEntity)entry.Entity;
            trackable.LastUpdate = DateTime.UtcNow;
        }
    }
}