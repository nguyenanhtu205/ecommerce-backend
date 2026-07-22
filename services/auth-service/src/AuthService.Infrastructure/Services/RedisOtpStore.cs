using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace AuthService.Infrastructure.Services;

public class RedisOtpStore(IConnectionMultiplexer redis) : IOtpStore
{
    public async Task SetCodeAsync(string email, string plainCode, string role, TimeSpan ttl, CancellationToken ct)
    {
        IDatabase db = redis.GetDatabase();
        string payload = JsonSerializer.Serialize(new CodeEntry(Hash(plainCode), role));
        await db.StringSetAsync(CodeKey(email), payload, ttl);
    }

    public async Task<OtpVerifyResult> VerifyCodeAsync(string email, string plainCode, CancellationToken ct)
    {
        IDatabase db = redis.GetDatabase();
        RedisValue stored = await db.StringGetAsync(CodeKey(email));

        if (stored.IsNullOrEmpty)
        {
            return new OtpVerifyResult(false, null);
        }

        CodeEntry? entry = JsonSerializer.Deserialize<CodeEntry>(stored.ToString());
        if (entry is null || entry.HashedCode != Hash(plainCode))
        {
            return new OtpVerifyResult(false, null);
        }

        await db.KeyDeleteAsync(CodeKey(email));
        return new OtpVerifyResult(true, entry.Role);
    }

    public async Task MarkVerifiedAsync(string email, string role, TimeSpan ttl, CancellationToken ct)
    {
        IDatabase db = redis.GetDatabase();
        await db.StringSetAsync(VerifiedKey(email), role, ttl);
    }

    public async Task<string?> GetVerifiedRoleAsync(string email, CancellationToken ct)
    {
        IDatabase db = redis.GetDatabase();
        RedisValue value = await db.StringGetAsync(VerifiedKey(email));
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task ClearVerifiedAsync(string email, CancellationToken ct)
    {
        IDatabase db = redis.GetDatabase();
        await db.KeyDeleteAsync(VerifiedKey(email));
    }

    private static string CodeKey(string email)
    {
        return $"otp:register:{email}";
    }

    private static string VerifiedKey(string email)
    {
        return $"otp:register:verified:{email}";
    }

    private static string Hash(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private record CodeEntry(string HashedCode, string Role);
}
