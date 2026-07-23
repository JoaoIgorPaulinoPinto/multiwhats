using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.enums;
using System.Text.Json;

namespace multiwhats_api.src.data.db;

/// <summary>
/// Contexto principal do banco de dados (Entity Framework Core).
/// 
/// PONTO CENTRAL DO SISTEMA: Todas as operações com o banco MySQL passam por aqui.
/// 
/// RESPONSABILIDADES:
/// 1. Mapear classes C# para tabelas MySQL (via DbSet<T>)
/// 2. Configurar relacionamentos, índices e regras do banco
/// 3. Aplicar AUDITORIA AUTOMÁTICA ao salvar (timestamps + soft delete)
/// 4. Gerar logs de auditoria detalhados (valores antigos/novos em JSON)
/// 
/// COMO FUNCIONA O SOFT DELETE:
/// - Nenhum registro é deletado de verdade. Quando alguém chama DELETE,
///   o campo "IsDeleted" vira true e o registro some das consultas.
/// - Isso preserva dados para auditoria e permite "desfazer" deletes.
/// 
/// COMO FUNCIONA O AUTO-AUDIT:
/// - Ao chamar SaveChanges()/SaveChangesAsync(), o método ApplyAudit()
///   é chamado automaticamente antes de salvar no banco.
/// - Ele preenche CreatedAt, LastUpdate, CreatedByUserId, LastUpdatedByUserId
///   e converte deletes em soft deletes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ═══════════════════════════════════════════════════════════════════
    // TABELAS DO BANCO DE DADOS
    // Cada DbSet<T> representa uma tabela no MySQL.
    // Ao consultar Clients, por exemplo, o EF Core gera: SELECT * FROM Clients
    // ═══════════════════════════════════════════════════════════════════
    public DbSet<Client> Clients { get; set; }       // Tabela de clientes (empresas)
    public DbSet<Chat> Chats { get; set; }            // Tabela de conversas WhatsApp
    public DbSet<Contact> Contacts { get; set; }      // Tabela de contatos WhatsApp
    public DbSet<Message> Messages { get; set; }      // Tabela de mensagens
    public DbSet<Occurrence> Occurrences { get; set; } // Tabela de chamados/ocorrências
    public DbSet<ClientTask> ClientTasks { get; set; } // Tabela de tarefas da empresa
    public DbSet<Group> Groups { get; set; }           // Tabela de grupos WhatsApp
    public DbSet<User> Users { get; set; }             // Tabela de usuários do sistema
    public DbSet<AuditLog> AuditLogs { get; set; }    // Tabela de logs de auditoria
    public DbSet<Device> Devices { get; set; }         // Tabela de dispositivos conectados

    /// <summary>
    /// ID do usuário que está realizando a operação atual.
    /// É definido pelo controller antes de salvar e usado pelo ApplyAudit()
    /// para saber quem criou/modificou/deletou o registro.
    /// Exemplo: no controller, _context.CurrentUserId = userId;
    /// </summary>
    public int? CurrentUserId { get; set; }

    /// <summary>
    /// Configuração do modelo do banco de dados.
    /// Aqui definimos: nomes das tabelas, tipos de colunas, relacionamentos,
    /// índices, conversões de enum para string, e filtros globais (soft delete).
    /// 
    /// O "model builder" é como um "arquiteto" que diz ao banco como estruturar as tabelas.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ─────────────────────────────────────────────────────────────
        // CLIENT - Empresa/Cliente que usa o sistema
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");                           // Nome da tabela no MySQL
            entity.Property(e => e.Status)
                  .HasConversion<string>()                       // Converte enum ClientStatus para string ("Active"/"Inactive")
                  .HasMaxLength(20);                             // Máximo 20 caracteres no banco
            entity.HasQueryFilter(e => !e.IsDeleted);            // FILTRO GLOBAL: esconde registros deletados automaticamente
        });

        // ─────────────────────────────────────────────────────────────
        // CHAT - Conversa do WhatsApp
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("Chats");
            entity.HasIndex(e => e.Jid).IsUnique();              // JID deve ser único (não pode ter dois chats com mesmo JID)
            entity.HasIndex(e => e.PhoneNumber);                 // Índice no telefone para buscas rápidas
            entity.HasOne(e => e.Contact)                        // Um Chat tem UM Contact (1:1)
                  .WithOne(c => c.Chat)                          // Um Contact tem UM Chat (1:1 inverso)
                  .HasForeignKey<Chat>(e => e.ContactId)         // FK no Chat
                  .OnDelete(DeleteBehavior.SetNull);             // Se o Contact for deletado, Chat.ContactId vira NULL (não deleta o chat)
            entity.HasOne(e => e.Client)                         // Um Chat pertence a UM Client (N:1)
                  .WithMany(c => c.Chats)                        // Um Client tem MUITOS Chats (1:N)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);             // Se Client for deletado, Chat.ClientId vira NULL
            entity.HasOne(e => e.AssignedTo)                     // Um Chat pode ter UM operador atribuído
                  .WithMany()
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete: esconde chats deletados
        });

        // ─────────────────────────────────────────────────────────────
        // CONTACT - Número de WhatsApp (pessoa ou entidade)
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.HasIndex(e => e.Jid).IsUnique();              // JID único (ex: 5511999999999@c.us)
            entity.HasIndex(e => e.PhoneNumber);                 // Índice para buscas por telefone
            entity.HasOne(e => e.Client)                         // Um Contact pode estar vinculado a UM Client
                  .WithMany(c => c.Contacts)                     // Um Client tem MUITOS Contacts
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);             // Se Client deletado, Contact.ClientId vira NULL
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // MESSAGE - Mensagem do WhatsApp
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasIndex(e => e.MessageId);                   // ID da mensagem no WhatsApp (para deduplicação)
            entity.HasIndex(e => e.PhoneNumber);                 // Índice para buscas por telefone
            entity.HasIndex(e => e.Timestamp);                   // Índice para ordenação por data
            entity.HasOne(e => e.Chat)                           // Uma Message pertence a UM Chat
                  .WithMany(c => c.Messages)                     // Um Chat tem MUITAS Messages
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);             // Se Chat deletado, todas as Messages são deletadas também
            entity.HasOne(e => e.ReplyTo)                        // Uma Message pode ser resposta de OUTRA Message
                  .WithMany()
                  .HasForeignKey(e => e.ReplyToId)
                  .OnDelete(DeleteBehavior.SetNull);             // Se a mensagem original for deletada, ReplyToId vira NULL
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // OCCURRENCE - Chamado/Incidente vinculado a um Chat
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Occurrence>(entity =>
        {
            entity.ToTable("Occurrences");
            entity.Property(e => e.Status)
                  .HasConversion<string>()                       // Converte enum OccurrenceStatus para string
                  .HasMaxLength(20);
            entity.Property(e => e.Priority)
                  .HasConversion<string>()                       // Converte enum Priority para string
                  .HasMaxLength(20);
            entity.HasOne(e => e.Chat)                           // Uma Occurrence pertence a UM Chat
                  .WithMany(c => c.Occurrences)                  // Um Chat tem MUITAS Occurrences
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);             // Se Chat deletado, Occurrences também são
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // CLIENTTASK - Tarefa/demanda da empresa
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<ClientTask>(entity =>
        {
            entity.ToTable("ClientTasks");
            entity.Property(e => e.Status)
                  .HasConversion<string>()                       // Converte enum ClientTaskStatus para string
                  .HasMaxLength(20);
            entity.Property(e => e.Priority)
                  .HasConversion<string>()                       // Converte enum Priority para string
                  .HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // GROUP - Grupo do WhatsApp
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // USER - Usuário do sistema (operador/agent)
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Role)
                  .HasConversion<string>()                       // Converte enum UserRole para string ("Support"/"Dev"/"Admin")
                  .HasMaxLength(20);
            entity.HasIndex(e => e.Name).IsUnique();             // Nome deve ser único (não pode ter dois usuários com mesmo nome)
            entity.HasQueryFilter(e => !e.IsDeleted);            // Soft delete
        });

        // ─────────────────────────────────────────────────────────────
        // DEVICE - Dispositivo WhatsApp conectado (singleton)
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasIndex(e => e.Jid);                         // Índice no JID para buscas rápidas
            // NOTA: Device NÃO tem HasQueryFilter porque NÃO herda BaseEntity.
            // É uma tabela singleton (só 1 registro) que representa o dispositivo conectado.
        });

        // ─────────────────────────────────────────────────────────────
        // AUDITLOG - Log de auditoria (não tem soft delete)
        // ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasIndex(e => e.Timestamp);                   // Índice para buscas por data
            entity.HasIndex(e => e.EntityType);                  // Índice para buscas por tipo de entidade
            // NOTA: AuditLog NÃO herda BaseEntity, então não tem soft delete.
            // Logs de auditoria nunca devem ser deletados (são permanentes).
        });
    }

    // ═══════════════════════════════════════════════════════════════════
    // SOBRESCRITA DE SaveChanges - Onde a mágica da auditoria acontece
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sobrescreve SaveChanges() para aplicar auditoria automática ANTES de salvar.
    /// Toda vez que alguém chama context.SaveChanges() ou SaveChangesAsync(),
    /// o método ApplyAudit() é chamado primeiro.
    /// </summary>
    public override int SaveChanges()
    {
        ApplyAudit();                                            // Aplica timestamps + soft delete
        return base.SaveChanges();                               // Salva no banco MySQL
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();                                            // Aplica timestamps + soft delete
        return base.SaveChangesAsync(cancellationToken);         // Salva no banco MySQL
    }

    /// <summary>
    /// MÉTODO CENTRAL DE AUDITORIA AUTOMÁTICA.
    /// 
    /// O que faz:
    /// 1. Percorre todas as entidades que estão sendo adicionadas, modificadas ou deletadas
    /// 2. Para cada uma, preenche os campos de auditoria (timestamps, userId)
    /// 3. Converte DELETE em SOFT DELETE (muda estado de Deleted para Modified + IsDeleted=true)
    /// 
    /// COMO FUNCIONA O "Change Tracker":
    /// - O Entity Framework Core rastreia todas as mudanças que você faz nas entidades
    /// - Quando você chama SaveChanges(), ele sabe: "fulano foi adicionado, siclano foi modificado, etc."
    /// - O ChangeTracker.Entries() retorna a lista de todas essas mudanças pendentes
    /// 
    /// EXEMPLO PRÁTICO:
    /// - Se você faz: context.Clients.Add(new Client("Timontec"))
    /// - O EF Core detecta: EntityState.Added para o Client
    /// - ApplyAudit() preenche: CreatedAt=agora, CreatedByUserId=usuário atual
    /// - Ao deletar: entity.IsDeleted=true (não remove do banco)
    /// </summary>
    private void ApplyAudit()
    {
        // Pega todas as entidades que estão sendo criadas, modificadas ou deletadas
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&                 // Só entidades que herdam de BaseEntity
                        (e.State == EntityState.Added ||         // Novos registros
                         e.State == EntityState.Modified ||      // Registros modificados
                         e.State == EntityState.Deleted));       // Registros que "deletaram" (serão convertidos em soft delete)

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;               // Converte para BaseEntity para acessar os campos de auditoria
            var now = DateTime.UtcNow;                           // Horário UTC (padrão para todos os sistemas)

            // SEMPRE atualiza o LastUpdate (data da última modificação)
            entity.LastUpdate = now;

            if (entry.State == EntityState.Added)
            {
                // ╔══════════════════════════════════════════════════╗
                // ║ NOVO REGISTRO: preenche CreatedAt + LastUpdate   ║
                // ║ CreatedByUserId = quem está criando agora        ║
                // ╚══════════════════════════════════════════════════╝
                entity.CreatedAt = now;
                entity.CreatedByUserId ??= CurrentUserId;        // ??= só atribui se ainda for null
                entity.LastUpdatedByUserId ??= CurrentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // ╔══════════════════════════════════════════════════╗
                // ║ MODIFICAÇÃO: só atualiza LastUpdatedByUserId     ║
                // ╚══════════════════════════════════════════════════╝
                entity.LastUpdatedByUserId = CurrentUserId;
            }
            else if (entry.State == EntityState.Deleted)
            {
                // ╔══════════════════════════════════════════════════╗
                // ║ DELETE: converte para SOFT DELETE                 ║
                // ║ Em vez de DELETAR do banco, marca IsDeleted=true ║
                // ║ e muda o estado para Modified (para o EF salvar) ║
                // ╚══════════════════════════════════════════════════╝
                entry.State = EntityState.Modified;              // Muda de Deleted para Modified (para o EF não deletar)
                entity.IsDeleted = true;                         // Marca como deletado (filtro global esconde)
                entity.LastUpdatedByUserId = CurrentUserId;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // GERAÇÃO DE LOGS DE AUDITORIA DETALHADOS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gera logs de auditoria DETALHADOS para cada operação (CREATE/UPDATE).
    /// 
    /// DIFERENÇA DO ApplyAudit():
    /// - ApplyAudit(): preenche timestamps e converte deletes em soft delete (automático)
    /// - GenerateAuditLogs(): gera LOGS DETALHADOS com valores antigos/novos em JSON
    /// 
    /// EXEMPLO DE LOG GERADO:
    /// {
    ///   "UserId": 1,
    ///   "UserName": "Joao",
    ///   "EntityType": "Client",
    ///   "EntityId": 5,
    ///   "Action": "Updated",
    ///   "Description": "Updated Client #5: [Name: Timontec -> Timontec LTDA]",
    ///   "OldValues": "{\"Name\": \"Timontec\"}",
    ///   "NewValues": "{\"Name\": \"Timontec LTDA\", \"Status\": \"Active\"}"
    /// }
    /// 
    /// IMPORTANTE: O campo "Password" é EXCLUÍDO dos logs por segurança (nunca logs a senha).
    /// </summary>
    public List<AuditLog> GenerateAuditLogs(int? userId, string? userName, string? userRole)
    {
        var logs = new List<AuditLog>();

        // Pega apenas entidades criadas ou modificadas (deletes são tratados pelo ApplyAudit)
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var entityType = entry.Entity.GetType().Name;        // Nome da classe (ex: "Client", "Message")
            var entityId = entry.Property("Id")?.CurrentValue?.ToString(); // ID do registro
            var action = entry.State == EntityState.Added ? "Created" : "Updated";

            // ── Valores ANTIGOS (antes da modificação) ──
            // Só existe para Updates, pois para Creates não havia valores anteriores
            var oldValues = entry.State == EntityState.Modified
                ? JsonSerializer.Serialize(
                    entry.OriginalValues.Properties
                        .Where(p => entry.OriginalValues[p] != entry.CurrentValues[p]) // Só inclui propriedades que mudaram
                        .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString()))
                : null;

            // ── Valores NOVOS (depois da modificação) ──
            // Exclui "Password" por segurança (nunca loga senhas!)
            var newValues = JsonSerializer.Serialize(
                entry.CurrentValues.Properties
                    .Where(p => p.Name != "Password")            // EXCLUI senha dos logs por segurança
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString()));

            // ── Descrição legível das mudanças ──
            // Exemplo: "Name: Timontec -> Timontec LTDA, Status: Active -> Inactive"
            var changedProps = entry.State == EntityState.Modified
                ? string.Join(", ",
                    entry.OriginalValues.Properties
                        .Where(p => entry.OriginalValues[p]?.ToString() != entry.CurrentValues[p]?.ToString() && p.Name != "LastUpdate")
                        .Select(p => $"{p.Name}: {entry.OriginalValues[p]} -> {entry.CurrentValues[p]}"))
                : null;

            // Monta a descrição final do log
            var description = action switch
            {
                "Created" => $"Created {entityType} #{entityId}",
                "Updated" => $"Updated {entityType} #{entityId}: [{changedProps}]",
                _ => $"{action} {entityType} #{entityId}"
            };

            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                EntityType = entityType,
                EntityId = entityId != null ? int.Parse(entityId) : null,
                Action = action,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow
            };

            logs.Add(log);

            // ── Log colorido no console para debug visual ──
            var color = action == "Created" ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.ForegroundColor = color;
            Console.WriteLine($"[AUDIT] {log.Timestamp:HH:mm:ss} | User \"{userName ?? "System"}\" ({userRole ?? "-"}) | {action} {entityType} #{entityId} | {description}");
            Console.ResetColor();
        }

        return logs;
    }
}
