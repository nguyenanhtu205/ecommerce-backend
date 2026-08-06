using Common.Contracts.Events;
using MassTransit;
using OrderService.Application.Consumers;
using OrderService.Application.Sagas.Checkout;
using OrderService.Application.Sagas.OrderReservation;
using OrderService.Infrastructure.Data;

namespace OrderService.API.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddOrderServiceMassTransit(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CancelOrderConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<CancelOrderConsumer>();

                rider.AddSagaStateMachine<CheckoutSaga, CheckoutSagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<ApplicationDbContext>();
                        r.UsePostgres();
                    });

                rider.AddSagaStateMachine<OrderReservationSaga, OrderReservationSagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<ApplicationDbContext>();
                        r.UsePostgres();
                    });

                rider.AddProducer<CheckoutInitiated>("checkout.initiated.v1");
                rider.AddProducer<ReserveOrderStock>("checkout.reserve-order-stock.v1");
                rider.AddProducer<RedeemVoucher>("promotion.redeem-voucher.v1");
                rider.AddProducer<ReleaseVoucher>("promotion.release-voucher.v1");
                rider.AddProducer<CreatePayment>("payment.create-payment.v1");
                rider.AddProducer<OrderPaymentSucceeded>("order.payment-succeeded.v1");
                rider.AddProducer<OrderPaymentFailed>("order.payment-failed.v1");
                rider.AddProducer<ReleaseStockCommand>("inventory.release-stock.v1");
                rider.AddProducer<CommitStockCommand>("inventory.commit-stock.v1");
                rider.AddProducer<ReserveStock>("inventory.reserve-stock.v1");
                rider.AddProducer<OrderStockReserved>("order.stock-reserved.v1");
                rider.AddProducer<OrderStockReservationFailed>("order.stock-reservation-failed.v1");
                rider.AddProducer<CreateShipment>("shipping.create-shipment.v1");
                rider.AddProducer<CancelOrder>("order.cancel-order.v1");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<CheckoutInitiated>(
                        "checkout.initiated.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                        });

                    k.TopicEndpoint<ReserveOrderStock>(
                        "checkout.reserve-order-stock.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<OrderStockReserved>(
                        "order.stock-reserved.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });

                    k.TopicEndpoint<OrderStockReservationFailed>(
                        "order.stock-reservation-failed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });

                    k.TopicEndpoint<StockReserved>(
                        "inventory.stock-reserved.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<StockReservationFailed>(
                        "inventory.stock-reservation-failed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<VoucherRedeemed>(
                        "promotion.voucher-redeemed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });

                    k.TopicEndpoint<VoucherRedemptionFailed>(
                        "promotion.voucher-redemption-failed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });

                    k.TopicEndpoint<VnPayPaymentConfirmed>(
                        "payment.vnpay-confirmed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });

                    k.TopicEndpoint<OrderPaymentSucceeded>(
                        "order.payment-succeeded.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<ShipmentCreated>(
                        "shipping.shipment-created.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<ShipmentCreationFailed>(
                        "shipping.shipment-creation-failed.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<OrderReservationSagaState>(context);
                        });

                    k.TopicEndpoint<CancelOrder>(
                        "order.cancel-order.v1", "order-service-group", e =>
                        {
                            e.ConfigureConsumer<CancelOrderConsumer>(context);
                            e.ConfigureSaga<OrderReservationSagaState>(context);

                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        });

                    k.TopicEndpoint<PaymentRedirectCreated>(
                        "payment.redirect-created.v1", "order-service-group", e =>
                        {
                            e.ConfigureSaga<CheckoutSagaState>(context);
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)));
                        });
                });
            });
        });

        return services;
    }
}
