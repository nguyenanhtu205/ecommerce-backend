using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Common.API.RateLimiting;

public record RateLimitPolicy(int PermitLimit, int WindowSeconds, int QueueLimit = 0);

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCommonRateLimiter(
        this IServiceCollection services,
        RateLimitPolicy get, RateLimitPolicy post, RateLimitPolicy put, RateLimitPolicy delete)
    {
        services.AddRateLimiter(options =>
        {
            AddPolicy(options, "get", get);
            AddPolicy(options, "post", post);
            AddPolicy(options, "put", put);
            AddPolicy(options, "delete", delete);
        });

        return services;
    }

    private static void AddPolicy(RateLimiterOptions options, string name, RateLimitPolicy policy)
    {
        options.AddFixedWindowLimiter(name, opt =>
        {
            opt.PermitLimit = policy.PermitLimit;
            opt.Window = TimeSpan.FromSeconds(policy.WindowSeconds);
            opt.QueueLimit = policy.QueueLimit;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    }
}
