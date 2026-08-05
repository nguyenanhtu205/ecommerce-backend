using Common.Contracts.Events;
using MassTransit;
using PromotionService.Application.Consumers;
using PromotionService.Infrastructure.Data;

namespace PromotionService.API.Infrastructure;

public static class MassTransitExtension
{
    public static IServiceCollection AddPromotionServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RedeemVoucherConsumer>();
            x.AddConsumer<ReleaseVoucherConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<RedeemVoucherConsumer>();
                rider.AddConsumer<ReleaseVoucherConsumer>();

                rider.AddProducer<VoucherRedeemed>("promotion.voucher-redeemed.v1");
                rider.AddProducer<VoucherRedemptionFailed>("promotion.voucher-redemption-failed.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<RedeemVoucher>(
                        "promotion.redeem-voucher.v1",
                        "promotion-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<RedeemVoucherConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });

                    k.TopicEndpoint<ReleaseVoucher>(
                        "promotion.release-voucher.v1",
                        "promotion-service-group",
                        e =>
                        {
                            e.ConfigureConsumer<ReleaseVoucherConsumer>(context);
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });
                });
            });
        });

        return services;
    }
}
