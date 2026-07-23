using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// FACTORY DO PADRÃO STRATEGY PARA MENSAGENS.
/// 
/// O QUE É O PADRÃO STRATEGY:
/// É um padrão de design que permite trocar a lógica de uma operação em tempo de execução,
/// sem precisar de if/else ou switch/case gigantes.
/// 
/// COMO FUNCIONA NESTE PROJETO:
/// - Cada tipo de mensagem (texto, imagem, áudio, etc.) tem sua própria classe Strategy
/// - A Factory escolhe a strategy correta com base no tipo de mensagem
/// - Isso mantém o código organizado e fácil de adicionar novos tipos
/// 
/// EXEMPLO PRÁTICO:
/// - Se o operador envia um TEXTO → usa TextMessageStrategy
/// - Se o operador envia uma IMAGEM → usa ImageMessageStrategy
/// - Se o operador envia um ÁUDIO → usa AudioMessageStrategy
/// 
/// ANTES (sem Strategy):
/// if (type == "text") { ... lógica de texto ... }
/// else if (type == "image") { ... lógica de imagem ... }
/// else if (type == "audio") { ... lógica de áudio ... }
/// // ... cada vez que adicionar um tipo novo, fica mais um if/else
/// 
/// DEPOIS (com Strategy):
/// var strategy = factory.Get(type);
/// strategy.BuildNodePayload(...);  // A strategy já sabe o que fazer
/// 
/// COMO ADICIONAR UM NOVO TIPO:
/// 1. Criar uma nova classe que implementa IMessageStrategy (ex: LocationMessageStrategy)
/// 2. Registrar no Program.cs: builder.Services.AddSingleton<IMessageStrategy, LocationMessageStrategy>();
/// 3. Pronto! A Factory já vai encontrar automaticamente
/// </summary>
public class MessageStrategyFactory
{
    // Dicionário que mapeia cada MessageType para sua strategy correspondente
    // Exemplo: MessageType.Text → TextMessageStrategy
    private readonly Dictionary<MessageType, IMessageStrategy> _strategies;

    /// <summary>
    /// Construtor que recebe TODAS as strategies registradas no DI (Dependency Injection).
    /// O ASP.NET Core injeta todas as classes que implementam IMessageStrategy.
    /// 
    /// Exemplo de registro no Program.cs:
    /// builder.Services.AddSingleton<IMessageStrategy, TextMessageStrategy>();
    /// builder.Services.AddSingleton<IMessageStrategy, ImageMessageStrategy>();
    /// // ... etc
    /// 
    /// O IEnumerable<IMessageStrategy> recebe todas essas instâncias automaticamente.
    /// </summary>
    public MessageStrategyFactory(IEnumerable<IMessageStrategy> strategies)
    {
        // Converte a lista para um dicionário: { Text → TextStrategy, Image → ImageStrategy, ... }
        _strategies = strategies.ToDictionary(s => s.Type);
    }

    /// <summary>
    /// Retorna a strategy correta para o tipo de mensagem informado.
    /// 
    /// EXEMPLO:
    /// var strategy = factory.Get(MessageType.Image);  // Retorna ImageMessageStrategy
    /// var payload = strategy.BuildNodePayload(jid, request);  // Monta payload de imagem
    /// 
    /// Se não encontrar a strategy (tipo não registrado), lança exceção.
    /// </summary>
    public IMessageStrategy Get(MessageType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
            return strategy;

        throw new ArgumentException($"Nenhuma strategy registrada para o MessageType: {type}");
    }
}
