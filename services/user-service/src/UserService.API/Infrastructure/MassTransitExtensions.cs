using Common.Contracts.Events;
using MassTransit;
using UserService.Application.Consumers;

namespace UserService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddUserServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<UserRegisteredConsumer>();

                rider.AddProducer<PickupAddressSnapshotUpdated>("user.pickup-address-snapshot-updated.v1");
                rider.AddProducer<AvatarMediaAttached>("user.avatar-media-attached.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<UserRegistered>(
                        "user.registered.v1",
                        "user-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<UserRegisteredConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
