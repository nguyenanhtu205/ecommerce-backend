using AuthService.Infrastructure.Data;
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
                rider.AddProducer<UserRegistered>("user.registered.v1");
                rider.AddProducer<OtpRequested>("notification.otp-requested.v1");

                rider.UsingKafka((_, k) =>
                {
                    k.Host(kafkaOptions.BootstrapServers);
                });
            });
        });

        return services;
    }
}
