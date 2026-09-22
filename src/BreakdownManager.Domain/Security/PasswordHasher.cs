using System.Security.Cryptography;

namespace BreakdownManager.Domain.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing using only the .NET base class library
/// (<see cref="Rfc2898DeriveBytes"/>) — no external package needed. Lives in Domain,
/// which has no project dependencies, so both the Data layer (seeding demo users)
/// and the Services layer (verifying a login) can use it without creating a
/// backward reference in the App → Services → Data → Domain dependency chain.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSizeBytes = 16;   // 128-bit salt
    private const int KeySizeBytes = 32;    // 256-bit derived key
    private const int Iterations = 100_000; // OWASP-recommended floor for PBKDF2-SHA256 (as of 2023)
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a plaintext password into a single self-describing string
    /// ("iterations.salt.key", base64) that's safe to store in <c>User.PasswordHash</c>.
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySizeBytes);

        return string.Join('.', Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
    }

    /// <summary>
    /// Verifies a plaintext password against a hash produced by <see cref="Hash"/>.
    /// Returns false (rather than throwing) for malformed or empty input so callers
    /// can treat any verification failure as "wrong credentials".
    /// </summary>
    public static bool Verify(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        var parts = hashedPassword.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt, expectedKey;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedKey = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedKey.Length);

        // Fixed-time comparison so a login attempt can't be timed to learn how much of the hash matched.
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
