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

            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<ProductCreatedConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<ProductCreated>(
                        "product-catalog.product-created.v1",
                        "inventory-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ProductCreatedConsumer>(context);

                            e.UseEntityFrameworkOutbox<ApplicationDbContext>(context);

                            e.UseMessageRetry(r =>
                                r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
