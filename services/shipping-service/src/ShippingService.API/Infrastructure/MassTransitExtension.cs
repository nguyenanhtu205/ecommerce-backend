using Common.Contracts.Events;
using MassTransit;
using ShippingService.Application.Consumers;
using ShippingService.Infrastructure.Data;

namespace ShippingService.API.Infrastructure;

public static class MassTransitExtension
{
    public static IServiceCollection AddShippingServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CreateShipmentConsumer>();
            
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<CreateShipmentConsumer>();

                rider.AddProducer<ShipmentCreated>("shipping.shipment-created.v1");
                rider.AddProducer<ShipmentCreationFailed>("shipping.shipment-creation-failed.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<CreateShipment>(
                        "shipping.create-shipment.v1",
                        "shipping-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<CreateShipmentConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
