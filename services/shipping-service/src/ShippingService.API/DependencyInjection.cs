using Common.API;
using Common.API.RateLimiting;
using ShippingService.API.Infrastructure;
using ShippingService.API.Services;

namespace ShippingService.API;

public static class DependencyInjection
{
    public static void AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        builder.Services.AddAuthentication();

        builder.Services.AddCors();

        builder.Services.AddShippingServiceMassTransit(builder.Configuration);

        builder.Services.AddCommonRateLimiter(
            new RateLimitPolicy(30, 10, 5),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(2, 10));

        builder.Services.AddAuthorization();

        builder.Services.AddCommonApi();

        builder.Services.AddGrpc();
    }
}
