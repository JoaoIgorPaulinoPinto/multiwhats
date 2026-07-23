using System.Collections.Concurrent;

namespace multiwhats_api.src.services;

/// <summary>
/// SERVIÇO DE BLACKLIST DE TOKENS (Lista Negra).
/// 
/// O QUE É:
/// - Permite "revogar" tokens antes deles expirarem
/// - Quando o usuário faz Logout, o ID do token (JTI) é adicionado a esta lista
/// - Mesmo que o token ainda esteja válido (não expirou), ele será recusado
/// 
/// COMO FUNCIONA:
/// 1. Usuário faz Logout
/// 2. O JTI (ID único) do token é extraído do JWT
/// 3. O JTI é adicionado ao dicionário _revokedTokens
/// 4. Em toda requisição, o sistema verifica: "Esse JTI está na blacklist?"
/// 5. Se estiver → token é rejeitado (mesmo que ainda não tenha expirado)
/// 
/// POR QUE USAR ConcurrentDictionary:
/// - É uma versão thread-safe do Dictionary (seguro para múltiplas threads)
/// - Como a API pode receber várias requisições ao mesmo tempo,
///   precisamos de uma estrutura que não tenha problemas de concorrência
/// 
/// LIMITAÇÃO:
/// - A blacklist fica em MEMÓRIA (não no banco de dados)
/// - Se o servidor reiniciar, a blacklist é perdida (tokens revogados voltam a funcionar)
/// - Para produção, deveria usar Redis ou banco de dados
/// </summary>
public class TokenBlacklistService
{
    // Dicionário thread-safe: { JTI → data de expiração do token }
    private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

    /// <summary>
    /// Adiciona um token à blacklist (revoga ele).
    /// 
    /// EXEMPLO:
    /// - JTI: "abc-123-def-456"
    /// - Expiry: 2024-01-15 14:00:00 (quando o token expiraria naturalmente)
    /// </summary>
    public void Revoke(string jti, DateTime expiry)
    {
        _revokedTokens.TryAdd(jti, expiry);  // TryAdd evita duplicatas
    }

    /// <summary>
    /// Verifica se um token foi revogado (está na blacklist).
    /// 
    /// USO: Chamado no Program.cs, no evento OnTokenValidated do JWT Bearer.
    /// Se o JTI estiver na blacklist, o token é rejeitado com a mensagem "Token foi revogado."
    /// </summary>
    public bool IsRevoked(string jti)
    {
        return _revokedTokens.ContainsKey(jti);
    }

    /// <summary>
    /// Remove da blacklist tokens que já expiraram naturalmente.
    /// 
    /// POR QUE EXISTE:
    /// - A blacklist cresce infinitamente (cada logout adiciona um JTI)
    /// - Tokens expirados não precisam mais ficar na blacklist
    /// - Este método limpa os tokens que já passaram da data de expiração
    /// 
    /// OBS: Este método NÃO é chamado automaticamente.
    /// Para funcionar, deveria ter um "background job" chamando periodicamente.
    /// Por enquanto, é uma melhoria futura pendente.
    /// </summary>
    public void Cleanup()
    {
        var now = DateTime.UtcNow;
        var expired = _revokedTokens.Where(kvp => kvp.Value <= now).Select(kvp => kvp.Key).ToList();
        foreach (var key in expired)
        {
            _revokedTokens.TryRemove(key, out _);
        }
    }
}
