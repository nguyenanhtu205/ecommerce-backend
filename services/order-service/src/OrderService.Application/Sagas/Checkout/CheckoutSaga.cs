using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Sagas.Checkout;

public class CheckoutSaga : MassTransitStateMachine<CheckoutSagaState>
{
    public CheckoutSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CheckoutInitiatedEvent, x => x.CorrelateById(m => m.Message.CheckoutBatchId));

        Event(() => OrderStockReservedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Event(() => OrderStockReservationFailedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Event(() => VoucherRedeemedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Event(() => VoucherRedemptionFailedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Event(() => VnPayPaymentConfirmedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Event(() => PaymentRedirectCreatedEvent, x =>
        {
            x.CorrelateById(m => m.Message.CheckoutBatchId);
            x.OnMissingInstance(m => m.Fault());
        });

        Initially(
            When(CheckoutInitiatedEvent)
                .Then(context =>
                {
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.OrderIds = context.Message.OrderIds;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.OrderShares = context.Message.OrderShares;
                    context.Saga.PaymentMethod = context.Message.PaymentMethod;
                    context.Saga.PlatformVoucherCode = context.Message.PlatformVoucherCode;
                    context.Saga.ShopVouchers = context.Message.ShopVouchers;
                    context.Saga.ReservedOrderIds = [];
                    context.Saga.VoucherRedeemed = false;
                })
                .TransitionTo(AwaitingAllReservations)
        );

        During(AwaitingAllReservations,
            When(OrderStockReservedEvent)
                .Then(context =>
                {
                    if (!context.Saga.ReservedOrderIds.Contains(context.Message.OrderId))
                    {
                        context.Saga.ReservedOrderIds.Add(context.Message.OrderId);
                    }
                })
                .IfElse(context => context.Saga.ReservedOrderIds.Count >= context.Saga.OrderIds.Count,
                    allReserved => allReserved
                        .ThenAsync(async context =>
                        {
                            ITopicProducer<RedeemVoucher> producer = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<ITopicProducer<RedeemVoucher>>();
                            await producer.Produce(
                                new RedeemVoucher(
                                    context.Saga.CorrelationId,
                                    context.Saga.BuyerId,
                                    context.Saga.PlatformVoucherCode,
                                    context.Saga.ShopVouchers,
                                    context.Saga.OrderShares),
                                context.CancellationToken);
                        })
                        .TransitionTo(AwaitingVoucherRedemption),
                    stillWaiting => stillWaiting),
            When(OrderStockReservationFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .ThenAsync(async context =>
                {
                    IServiceProvider provider = context.GetPayload<IServiceProvider>();
                    ITopicProducer<ReleaseStockCommand> releaseStockProducer =
                        provider.GetRequiredService<ITopicProducer<ReleaseStockCommand>>();
                    ITopicProducer<CancelOrder> cancelOrderProducer =
                        provider.GetRequiredService<ITopicProducer<CancelOrder>>();

                    foreach (Guid orderId in context.Saga.ReservedOrderIds)
                    {
                        await releaseStockProducer.Produce(
                            new ReleaseStockCommand(orderId), context.CancellationToken);
                    }

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        await cancelOrderProducer.Produce(
                            new CancelOrder(orderId, context.Saga.FailReason ?? "stock reservation failed", "system"),
                            context.CancellationToken);
                    }
                })
                .TransitionTo(Cancelled)
        );

        During(AwaitingVoucherRedemption,
            When(VoucherRedeemedEvent)
                .Then(context => context.Saga.VoucherRedeemed = true)
                .ThenAsync(async context =>
                {
                    ITopicProducer<CreatePayment> producer = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<ITopicProducer<CreatePayment>>();
                    await producer.Produce(
                        new CreatePayment(
                            context.Saga.CorrelationId,
                            context.Saga.BuyerId,
                            context.Saga.TotalAmount,
                            context.Saga.PaymentMethod,
                            context.Saga.OrderShares),
                        context.CancellationToken);
                })
                .IfElse(context => context.Saga.PaymentMethod == "Cod",
                    cod => cod
                        .ThenAsync(async context =>
                        {
                            ITopicProducer<OrderPaymentSucceeded> producer = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<ITopicProducer<OrderPaymentSucceeded>>();
                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                await producer.Produce(
                                    new OrderPaymentSucceeded(context.Saga.CorrelationId, orderId),
                                    context.CancellationToken);
                            }
                        })
                        .TransitionTo(Completed),
                    vnpay => vnpay.TransitionTo(AwaitingPayment)),
            When(VoucherRedemptionFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .ThenAsync(async context =>
                {
                    IServiceProvider provider = context.GetPayload<IServiceProvider>();
                    ITopicProducer<ReleaseStockCommand> releaseStockProducer =
                        provider.GetRequiredService<ITopicProducer<ReleaseStockCommand>>();
                    ITopicProducer<CancelOrder> cancelOrderProducer =
                        provider.GetRequiredService<ITopicProducer<CancelOrder>>();

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        await releaseStockProducer.Produce(
                            new ReleaseStockCommand(orderId), context.CancellationToken);
                    }

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        await cancelOrderProducer.Produce(
                            new CancelOrder(orderId, context.Saga.FailReason ?? "voucher redemption failed", "system"),
                            context.CancellationToken);
                    }
                })
                .TransitionTo(Cancelled)
        );

        During(AwaitingPayment,
            When(PaymentRedirectCreatedEvent)
                .Then(context => context.Saga.RedirectUrl = context.Message.RedirectUrl),
            When(VnPayPaymentConfirmedEvent)
                .IfElse(context => context.Message.Success,
                    success => success
                        .ThenAsync(async context =>
                        {
                            ITopicProducer<OrderPaymentSucceeded> producer = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<ITopicProducer<OrderPaymentSucceeded>>();
                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                await producer.Produce(
                                    new OrderPaymentSucceeded(context.Saga.CorrelationId, orderId),
                                    context.CancellationToken);
                            }
                        })
                        .TransitionTo(Completed),
                    failed => failed
                        .Then(context => context.Saga.FailReason = context.Message.Reason)
                        .ThenAsync(async context =>
                        {
                            IServiceProvider provider = context.GetPayload<IServiceProvider>();
                            ITopicProducer<OrderPaymentFailed> orderPaymentFailedProducer =
                                provider.GetRequiredService<ITopicProducer<OrderPaymentFailed>>();
                            ITopicProducer<ReleaseVoucher> releaseVoucherProducer =
                                provider.GetRequiredService<ITopicProducer<ReleaseVoucher>>();
                            ITopicProducer<ReleaseStockCommand> releaseStockProducer =
                                provider.GetRequiredService<ITopicProducer<ReleaseStockCommand>>();
                            ITopicProducer<CancelOrder> cancelOrderProducer =
                                provider.GetRequiredService<ITopicProducer<CancelOrder>>();

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                await orderPaymentFailedProducer.Produce(
                                    new OrderPaymentFailed(context.Saga.CorrelationId, orderId,
                                        context.Saga.FailReason ?? "vnpay failed"),
                                    context.CancellationToken);
                            }

                            if (context.Saga.VoucherRedeemed)
                            {
                                await releaseVoucherProducer.Produce(
                                    new ReleaseVoucher(context.Saga.CorrelationId, context.Saga.OrderIds),
                                    context.CancellationToken);
                            }

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                await releaseStockProducer.Produce(
                                    new ReleaseStockCommand(orderId), context.CancellationToken);
                            }

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                await cancelOrderProducer.Produce(
                                    new CancelOrder(orderId, context.Saga.FailReason ?? "vnpay failed", "system"),
                                    context.CancellationToken);
                            }
                        })
                        .TransitionTo(Cancelled))
        );

        SetCompletedWhenFinalized();
    }

    public State AwaitingAllReservations { get; } = null!;
    public State AwaitingVoucherRedemption { get; } = null!;
    public State AwaitingPayment { get; } = null!;
    public State Completed { get; } = null!;
    public State Cancelled { get; } = null!;

    public Event<CheckoutInitiated> CheckoutInitiatedEvent { get; } = null!;
    public Event<OrderStockReserved> OrderStockReservedEvent { get; } = null!;
    public Event<OrderStockReservationFailed> OrderStockReservationFailedEvent { get; } = null!;
    public Event<VoucherRedeemed> VoucherRedeemedEvent { get; } = null!;
    public Event<VoucherRedemptionFailed> VoucherRedemptionFailedEvent { get; } = null!;
    public Event<VnPayPaymentConfirmed> VnPayPaymentConfirmedEvent { get; } = null!;
    public Event<PaymentRedirectCreated> PaymentRedirectCreatedEvent { get; } = null!;
}
