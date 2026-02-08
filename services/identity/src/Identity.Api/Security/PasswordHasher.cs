using System.Security.Cryptography;

namespace KubeCart.Identity.Api.Security;

public static class PasswordHasher
{
    private const int SaltSize = 32;         // bytes
    private const int HashSize = 64;         // bytes
    private const int Iterations = 100_000;  // good baseline for PBKDF2

    public static (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (hash, salt);
    }

    public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
    {
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            password,
            storedSalt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(computed, storedHash);
    }
}
