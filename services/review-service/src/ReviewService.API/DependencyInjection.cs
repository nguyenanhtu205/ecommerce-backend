using Common.API;
using Common.API.RateLimiting;
using ReviewService.API.Services;

namespace ReviewService.API;

public static class DependencyInjection
{
    public static void AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        builder.Services.AddCors();

        builder.Services.AddAuthentication();


        builder.Services.AddCommonRateLimiter(
            new RateLimitPolicy(30, 10, 5),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(2, 10));

        builder.Services.AddAuthorization();

        builder.Services.AddCommonApi();
    }
}
