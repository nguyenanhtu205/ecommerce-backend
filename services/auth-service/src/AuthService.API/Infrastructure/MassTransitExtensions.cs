using AuthService.Application.Consumers;
using Common.Contracts.Events;
using MassTransit;

namespace AuthService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddAuthServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        KafkaOptions kafkaOptions = configuration
            .GetSection(KafkaOptions.SectionName)
            .Get<KafkaOptions>()!;

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ShopCreatedConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<ShopCreatedConsumer>();

                rider.AddProducer<UserRegistered>("user.registered.v1");
                rider.AddProducer<OtpRequested>("notification.otp-requested.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(kafkaOptions.BootstrapServers);

                    k.TopicEndpoint<ShopCreated>(
                        "seller.shop-created.v1",
                        "auth-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ShopCreatedConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
