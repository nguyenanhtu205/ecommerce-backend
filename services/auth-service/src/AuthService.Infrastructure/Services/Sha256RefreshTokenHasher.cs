using System.Text;

namespace AuthService.Infrastructure.Services;

public class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool Verify(string token, string hash)
    {
        string computedHash = Hash(token);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(hash)
        );
    }
}
