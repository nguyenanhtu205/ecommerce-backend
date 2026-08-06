using MassTransit;

namespace ReviewService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddReviewServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.UsingKafka((_, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);
                });
            });
        });

        return services;
    }
}
