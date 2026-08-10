using Common.Contracts.Events;
using MassTransit;
using ProductCatalogService.Application.Consumers;

namespace ProductCatalogService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddProductCatalogServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReviewAggregateUpdatedConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<ReviewAggregateUpdatedConsumer>();

                rider.AddProducer<ProductCreated>("product-catalog.product-created.v1");
                rider.AddProducer<ProductListingViewUpdated>("product-catalog.product-listing-view-updated.v1");
                rider.AddProducer<ProductMediaAttached>("product-catalog.product-media-attached.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<ReviewAggregateUpdated>(
                        "review.review-aggregate-updated.v1",
                        "product-catalog-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ReviewAggregateUpdatedConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
