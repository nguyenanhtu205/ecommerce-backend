using Common.API;
using Common.API.RateLimiting;
using PaymentService.API.Infrastructure;
using PaymentService.API.Services;

namespace PaymentService.API;

public static class DependencyInjection
{
    public static void AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        builder.Services.AddAuthentication();

        builder.Services.AddCors();

        builder.Services.AddPaymentServiceMassTransit(builder.Configuration);

        builder.Services.AddCommonRateLimiter(
            new RateLimitPolicy(30, 10, 5),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(5, 10, 2),
            new RateLimitPolicy(2, 10));

        builder.Services.AddAuthorization();

        builder.Services.AddCommonApi();
    }
}
