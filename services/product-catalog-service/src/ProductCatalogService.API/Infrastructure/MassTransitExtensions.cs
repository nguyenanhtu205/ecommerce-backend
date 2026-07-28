using Common.Contracts.Events;
using MassTransit;

namespace ProductCatalogService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddProductCatalogServiceMassTransit(
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
                rider.AddProducer<ProductCreated>("product-catalog.product-created.v1");
                rider.AddProducer<ProductListingViewUpdated>("product-catalog.product-listing-view-updated.v1");

                rider.UsingKafka((_, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);
                });
            });
        });

        return services;
    }
}
