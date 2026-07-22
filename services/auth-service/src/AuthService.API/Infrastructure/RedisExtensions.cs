using AuthService.Application.Common.Interfaces;
using AuthService.Infrastructure.Services;
using StackExchange.Redis;

namespace AuthService.API.Infrastructure;

public static class RedisExtensions
{
    public static IServiceCollection AddAuthServiceRedis(
        this IServiceCollection services, IConfiguration configuration)
    {
        RedisOptions redisOptions = configuration
            .GetSection(RedisOptions.SectionName)
            .Get<RedisOptions>()!;

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        services.AddScoped<IOtpStore, RedisOtpStore>();

        return services;
    }
}
