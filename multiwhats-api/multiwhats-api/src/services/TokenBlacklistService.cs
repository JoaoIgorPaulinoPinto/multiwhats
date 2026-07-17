using System.Collections.Concurrent;

namespace multiwhats_api.src.services;

public class TokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

    public void Revoke(string jti, DateTime expiry)
    {
        _revokedTokens.TryAdd(jti, expiry);
    }

    public bool IsRevoked(string jti)
    {
        return _revokedTokens.ContainsKey(jti);
    }

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
