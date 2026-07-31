using BCrypt.Net;

namespace multiwhats_api.src.helpers;

// Hashes and verifies user passwords using BCrypt.
public static class PasswordHelper
{
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public static bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    // True if the stored value is already a BCrypt hash (vs. legacy plaintext).
    public static bool IsHashed(string value)
    {
        return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
    }
}
