using Common.Contracts.Events;
using MassTransit;
using SellerService.Infrastructure.Data;

namespace SellerService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddSellerServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
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
                rider.AddProducer<ShopActivated>("notification.shop-activated.v1");

                rider.UsingKafka((_, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);
                });
            });
        });

        return services;
    }
}
