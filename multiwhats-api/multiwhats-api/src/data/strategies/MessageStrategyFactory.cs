using multiwhats_api.src.data.enums;
using multiwhats_api.src.services;

namespace multiwhats_api.src.data.strategies;

// Factory for the Strategy pattern. Maps each MessageType to its IMessageStrategy implementation.
public class MessageStrategyFactory
{
    private readonly Dictionary<MessageType, IMessageStrategy> _strategies;
    private readonly SystemConfigService _config;

    public MessageStrategyFactory(IEnumerable<IMessageStrategy> strategies, SystemConfigService config)
    {
        _strategies = strategies.ToDictionary(s => s.Type);
        _config = config;
    }

    // Returns the strategy for the given message type. Throws if not registered or not allowed.
    public async Task<IMessageStrategy> Get(MessageType type)
    {
        if (type != MessageType.Text && type != MessageType.Contact && type != MessageType.Location && type != MessageType.Unknown)
        {
            var typeName = type.ToString();
            var allowed = await _config.GetListAsync("Media:AllowedTypes", new List<string> { "Image", "Audio", "Video", "Document", "Sticker" });
            if (!allowed.Contains(typeName))
                throw new ArgumentException($"Tipo de mídia não permitido: {type}. Tipos permitidos: {string.Join(", ", allowed)}");
        }

        if (_strategies.TryGetValue(type, out var strategy))
            return strategy;

        throw new ArgumentException($"Nenhuma strategy registrada para o MessageType: {type}");
    }

    public IMessageStrategy GetSync(MessageType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
            return strategy;

        throw new ArgumentException($"Nenhuma strategy registrada para o MessageType: {type}");
    }
}
