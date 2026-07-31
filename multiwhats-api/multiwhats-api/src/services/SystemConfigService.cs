using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.services;

public class SystemConfigService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private ConcurrentDictionary<string, string?> _cache = new();
    private DateTime _lastLoad = DateTime.MinValue;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static readonly List<(string key, string value, string type, string group, string description)> Defaults =
    [
        ("Auth:PasswordMinLength", "6", "Int", "Auth", "Minimum password length"),
        ("Auth:RequireRegistrationCode", "true", "Bool", "Auth", "Require registration code for signup"),
        ("Auth:RegistrationCodeExpiryHours", "48", "Int", "Auth", "Hours until a registration code expires"),
        ("Occurrence:StatusFlow", """["Open","InProgress","Resolved","Closed"]""", "JsonList", "Occurrence", "Allowed occurrence status flow sequence"),
        ("Media:AllowedTypes", """["Image","Audio","Video","Document","Sticker"]""", "JsonList", "Media", "Allowed media types for messages"),
        ("Media:UnsupportedMessage", "Desculpe, não consigo processar este tipo de mídia. Por favor, envie apenas texto, imagens, vídeos ou documentos.", "String", "Media", "Auto-reply sent when an unsupported media type is received"),
        ("Replies:SenderName", "", "String", "Replies", "Name shown as sender on automatic replies"),
        ("Business:Enabled", "false", "Bool", "Business", "Enable business hours and working days checking"),
        ("Business:OpenTime", "08:00", "String", "Business", "Opening time (format HH:mm)"),
        ("Business:CloseTime", "18:00", "String", "Business", "Closing time (format HH:mm)"),
        ("Business:WorkingDays", """["Monday","Tuesday","Wednesday","Thursday","Friday"]""", "JsonList", "Business", "Working days (English names: Monday, Tuesday, ...)"),
        ("Business:OutsideHoursMessage", "Olá! Nosso atendimento funciona de {days} das {open} às {close}. Recebemos sua mensagem e retornaremos no próximo horário de funcionamento.", "String", "Business", "Auto-reply for messages received outside business hours"),
        ("Business:Timezone", "America/Sao_Paulo", "String", "Business", "Timezone used to compute business hours"),
    ];

    public SystemConfigService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    public async Task SeedDefaultParametersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISystemParameterRepository>();
        var existing = await repo.GetAllAsync();
        var existingKeys = new HashSet<string>(existing.Select(p => p.Key), StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value, type, group, description) in Defaults)
        {
            if (existingKeys.Contains(key)) continue;

            var param = new SystemParameter(key, value, type, group, description, false);
            await repo.AddAsync(param);
        }
    }

    public async Task LoadAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISystemParameterRepository>();
        var parameters = await repo.GetAllAsync();
        _cache = new ConcurrentDictionary<string, string?>(
            parameters.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase)
        );
        _lastLoad = DateTime.UtcNow;
    }

    public void InvalidateCache()
    {
        _lastLoad = DateTime.MinValue;
    }

    private async Task EnsureLoadedAsync()
    {
        if (DateTime.UtcNow - _lastLoad > CacheTtl || _cache.IsEmpty)
            await LoadAsync();
    }

    private string? Resolve(string key)
    {
        return _cache.TryGetValue(key, out var val) ? val : _configuration[key];
    }

    public async Task<string?> GetStringAsync(string key, string? defaultValue = null)
    {
        await EnsureLoadedAsync();
        return Resolve(key) ?? defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue = 0)
    {
        var val = await GetStringAsync(key);
        return int.TryParse(val, out var result) ? result : defaultValue;
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var val = await GetStringAsync(key);
        return bool.TryParse(val, out var result) ? result : defaultValue;
    }

    public async Task<List<string>> GetListAsync(string key, List<string>? defaultValues = null)
    {
        var val = await GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(val))
            return defaultValues ?? new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(val) ?? defaultValues ?? new List<string>(); }
        catch { return defaultValues ?? new List<string>(); }
    }
}
