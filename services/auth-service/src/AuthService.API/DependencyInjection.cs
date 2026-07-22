using AuthService.API.Infrastructure;
using AuthService.API.Services;
using Common.API;
using Common.API.RateLimiting;

namespace AuthService.API;

public static class DependencyInjection
{
    public static void AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        builder.Services.AddCors();

        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.Services.AddAuthServiceRedis(builder.Configuration);

        builder.Services.AddAuthServiceMassTransit(builder.Configuration);

        builder.Services.AddCommonRateLimiter(
            new RateLimitPolicy(30, 10, 5),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(2, 10));

        builder.Services.AddAuthorization();

        builder.Services.AddCommonApi();
    }
}
