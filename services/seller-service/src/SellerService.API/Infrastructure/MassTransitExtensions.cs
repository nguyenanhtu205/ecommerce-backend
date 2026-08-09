using Common.Contracts.Events;
using MassTransit;
using SellerService.Application.Consumers;

namespace SellerService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddSellerServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PickupAddressSnapshotUpdatedConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<PickupAddressSnapshotUpdatedConsumer>();

                rider.AddProducer<ShopActivated>("notification.shop-activated.v1");
                rider.AddProducer<ShopCreated>("seller.shop-created.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<PickupAddressSnapshotUpdated>(
                        "user.pickup-address-snapshot-updated.v1",
                        "seller-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<PickupAddressSnapshotUpdatedConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
