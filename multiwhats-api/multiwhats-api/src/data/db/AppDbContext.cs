using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.enums;
using System.Text.Json;

namespace multiwhats_api.src.data.db;

// Main EF Core database context. Maps entities to PostgreSQL, applies auto-audit (timestamps, soft delete).
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Occurrence> Occurrences { get; set; }
    public DbSet<ClientTask> ClientTasks { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<RegistrationCode> RegistrationCodes { get; set; }
    public DbSet<SystemParameter> SystemParameters { get; set; }

    // Set by the controller before SaveChanges to track who created/modified/deleted.
    public int? CurrentUserId { get; set; }

    // Configures table names, column types, relationships, indexes, enum-to-string conversions, and global soft-delete filters.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("chats");
            entity.HasIndex(e => e.Jid).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.Property(e => e.ProfilePicUrl).HasMaxLength(EntityConstraints.ChatProfilePicUrlMaxLength);
            entity.HasOne(e => e.Contact)
                  .WithOne(c => c.Chat)
                  .HasForeignKey<Chat>(e => e.ContactId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Chats)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedTo)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasIndex(e => e.Jid).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Contacts)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasIndex(e => e.WhatssAppMessageId)
                  .IsUnique()
                  .HasFilter("\"whatsapp_message_id\" IS NOT NULL");
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.Chat)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ReplyTo)
                  .WithMany()
                  .HasForeignKey(e => e.ReplyToId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Occurrence>(entity =>
        {
            entity.ToTable("occurrences");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(e => e.Chat)
                  .WithMany(c => c.Occurrences)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ClientTask>(entity =>
        {
            entity.ToTable("client_tasks");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("groups");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.HasIndex(e => e.Jid);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EntityType);
        });

        modelBuilder.Entity<RegistrationCode>(entity =>
        {
            entity.ToTable("registration_codes");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SystemParameter>(entity =>
        {
            entity.ToTable("system_parameters");
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }

    // Applies auto-audit timestamps and soft delete before saving.
    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Applies auto-audit: fills timestamps/userId, converts deletes to soft deletes.
    private void ApplyAudit()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.UtcNow;

            entity.LastUpdate = now;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.CreatedByUserId ??= CurrentUserId;
                entity.LastUpdatedByUserId ??= CurrentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.LastUpdatedByUserId = CurrentUserId;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.LastUpdatedByUserId = CurrentUserId;
            }
        }
    }

    // Generates detailed audit logs with old/new JSON values for each change. Excludes Password field.
    public List<AuditLog> GenerateAuditLogs(int? userId, string? userName, string? userRole)
    {
        var logs = new List<AuditLog>();

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id")?.CurrentValue?.ToString();
            var action = entry.State == EntityState.Added ? "Created" : "Updated";

            var oldValues = entry.State == EntityState.Modified
                ? JsonSerializer.Serialize(
                    entry.OriginalValues.Properties
                        .Where(p => entry.OriginalValues[p] != entry.CurrentValues[p])
                        .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString()))
                : null;

            var newValues = JsonSerializer.Serialize(
                entry.CurrentValues.Properties
                    .Where(p => p.Name != "Password")
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString()));

            var changedProps = entry.State == EntityState.Modified
                ? string.Join(", ",
                    entry.OriginalValues.Properties
                        .Where(p => entry.OriginalValues[p]?.ToString() != entry.CurrentValues[p]?.ToString() && p.Name != "LastUpdate")
                        .Select(p => $"{p.Name}: {entry.OriginalValues[p]} -> {entry.CurrentValues[p]}"))
                : null;

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

            var color = action == "Created" ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.ForegroundColor = color;
            Console.WriteLine($"[AUDIT] {log.Timestamp:HH:mm:ss} | User \"{userName ?? "System"}\" ({userRole ?? "-"}) | {action} {entityType} #{entityId} | {description}");
            Console.ResetColor();
        }

        return logs;
    }
}
