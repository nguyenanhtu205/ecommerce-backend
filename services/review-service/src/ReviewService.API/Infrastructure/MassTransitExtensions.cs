using Common.Contracts.Events;
using MassTransit;
using ReviewService.Application.Consumers;

namespace ReviewService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddReviewServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCompletedConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<OrderCompletedConsumer>();

                rider.AddProducer<ReviewAggregateUpdated>("review.review-aggregate-updated.v1");
                rider.AddProducer<ReviewMediaAttached>("review.review-media-attached.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<OrderCompleted>(
                        "order.order-completed.v1",
                        "review-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<OrderCompletedConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
