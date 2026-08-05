using Common.Contracts.Events;
using InventoryService.Application.Consumers;
using InventoryService.Infrastructure.Data;
using MassTransit;

namespace InventoryService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddInventoryServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProductCreatedConsumer>();
            x.AddConsumer<ReserveStockConsumer>();
            x.AddConsumer<ReleaseStockCommandConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<ProductCreatedConsumer>();
                rider.AddConsumer<ReserveStockConsumer>();
                rider.AddConsumer<ReleaseStockCommandConsumer>();

                rider.AddProducer<StockReserved>("inventory.stock-reserved.v1");
                rider.AddProducer<StockReservationFailed>("inventory.stock-reservation-failed.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<ProductCreated>(
                        "product-catalog.product-created.v1",
                        "inventory-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ProductCreatedConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });

                    k.TopicEndpoint<ReserveStock>(
                        "inventory.reserve-stock.v1",
                        "inventory-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ReserveStockConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });

                    k.TopicEndpoint<ReleaseStockCommand>(
                        "inventory.release-stock.v1",
                        "inventory-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ReleaseStockCommandConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
