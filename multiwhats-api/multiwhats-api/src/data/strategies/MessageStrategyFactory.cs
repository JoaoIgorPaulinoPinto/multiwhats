using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Factory for the Strategy pattern. Maps each MessageType to its IMessageStrategy implementation.
public class MessageStrategyFactory
{
    private readonly Dictionary<MessageType, IMessageStrategy> _strategies;

    public MessageStrategyFactory(IEnumerable<IMessageStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Type);
    }

    // Returns the strategy for the given message type. Throws if not registered.
    public IMessageStrategy Get(MessageType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
            return strategy;

        throw new ArgumentException($"Nenhuma strategy registrada para o MessageType: {type}");
    }
}
