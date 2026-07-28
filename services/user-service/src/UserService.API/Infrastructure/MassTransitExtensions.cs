using Common.Contracts.Events;
using MassTransit;
using UserService.Application.Consumers;
using UserService.Infrastructure.Data;

namespace UserService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddUserServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();

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
                rider.AddConsumer<UserRegisteredConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<UserRegistered>(
                        "user.registered.v1",
                        "user-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<UserRegisteredConsumer>(context);

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
