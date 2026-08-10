using Common.Contracts.Events;
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
                rider.AddProducer<ReviewAggregateUpdated>("review.review-aggregate-updated.v1");

                rider.UsingKafka((_, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);
                });
            });
        });

        return services;
    }
}
