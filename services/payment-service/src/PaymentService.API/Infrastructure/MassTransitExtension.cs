using Common.Contracts.Events;
using MassTransit;
using PaymentService.Application.Consumers;

namespace PaymentService.API.Infrastructure;

public static class MassTransitExtension
{
    public static IServiceCollection AddPaymentServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CreatePaymentConsumer>();
            x.AddConsumer<OrderDeliveredConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<CreatePaymentConsumer>();
                rider.AddConsumer<OrderDeliveredConsumer>();

                rider.AddProducer<PaymentRedirectCreated>("payment.redirect-created.v1");
                rider.AddProducer<VnPayPaymentConfirmed>("payment.vnpay-confirmed.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<CreatePayment>(
                        "payment.create-payment.v1",
                        "payment-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<CreatePaymentConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });

                    k.TopicEndpoint<OrderDelivered>(
                        "shipping.order-delivered.v1",
                        "payment-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<OrderDeliveredConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
