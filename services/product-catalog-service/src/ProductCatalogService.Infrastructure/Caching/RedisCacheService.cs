using System.Text.Json;

namespace ProductCatalogService.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        IDatabase database = redis.GetDatabase();
        RedisValue value = await database.StringGetAsync(key);

        return value.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<T>((string)value!, SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration,
        CancellationToken cancellationToken = default)
        where T : class
    {
        IDatabase database = redis.GetDatabase();
        string serialized = JsonSerializer.Serialize(value, SerializerOptions);

        await database.StringSetAsync(key, serialized, expiration);
    }
}
