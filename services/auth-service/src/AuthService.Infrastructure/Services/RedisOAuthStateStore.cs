using StackExchange.Redis;

namespace AuthService.Infrastructure.Services;

public class RedisOAuthStateStore(IConnectionMultiplexer redis) : IOAuthStateStore
{
    public async Task SetStateAsync(string state, TimeSpan ttl, CancellationToken cancellationToken)
    {
        IDatabase db = redis.GetDatabase();
        await db.StringSetAsync(StateKey(state), "1", ttl);
    }

    public async Task<bool> ConsumeStateAsync(string state, CancellationToken cancellationToken)
    {
        IDatabase db = redis.GetDatabase();
        RedisValue value = await db.StringGetDeleteAsync(StateKey(state));
        return !value.IsNullOrEmpty;
    }

    private static string StateKey(string state)
    {
        return $"oauth:google:state:{state}";
    }
}
