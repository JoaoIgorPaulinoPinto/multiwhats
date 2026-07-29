using System.Collections.Concurrent;

namespace multiwhats_api.src.services;

// In-memory token blacklist for revoking JWTs before expiry. Uses ConcurrentDictionary for thread safety.
// NOTE: Blacklist is lost on server restart. For production, use Redis or a database.
public class TokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

    // Adds a token JTI to the blacklist.
    public void Revoke(string jti, DateTime expiry)
    {
        _revokedTokens.TryAdd(jti, expiry);
    }

    public bool IsRevoked(string jti)
    {
        return _revokedTokens.ContainsKey(jti);
    }

    // Removes expired tokens from the blacklist. Not called automatically - needs a background job.
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
