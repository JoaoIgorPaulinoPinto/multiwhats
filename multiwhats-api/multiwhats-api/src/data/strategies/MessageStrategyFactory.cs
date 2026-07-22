using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

public class MessageStrategyFactory
{
    private readonly Dictionary<MessageType, IMessageStrategy> _strategies;

    public MessageStrategyFactory(IEnumerable<IMessageStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Type);
    }

    public IMessageStrategy Get(MessageType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
            return strategy;

        throw new ArgumentException($"Nenhuma strategy registrada para o MessageType: {type}");
    }
}
