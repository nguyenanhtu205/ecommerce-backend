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
                        .Then(context =>
                        {
                            IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<IOutboxWriter>();
                            outboxWriter.Enqueue(new RedeemVoucher(
                                context.Saga.CorrelationId,
                                context.Saga.BuyerId,
                                context.Saga.PlatformVoucherCode,
                                context.Saga.ShopVouchers,
                                context.Saga.OrderShares));
                        })
                        .TransitionTo(AwaitingVoucherRedemption),
                    stillWaiting => stillWaiting),
            When(OrderStockReservationFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();

                    foreach (Guid orderId in context.Saga.ReservedOrderIds)
                    {
                        outboxWriter.Enqueue(new ReleaseStockCommand(orderId));
                    }

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        outboxWriter.Enqueue(new CancelOrder(
                            orderId, context.Saga.FailReason ?? "stock reservation failed", "system"));
                    }
                })
                .TransitionTo(Cancelled)
        );

        During(AwaitingVoucherRedemption,
            When(VoucherRedeemedEvent)
                .Then(context => context.Saga.VoucherRedeemed = true)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();
                    outboxWriter.Enqueue(new CreatePayment(
                        context.Saga.CorrelationId,
                        context.Saga.BuyerId,
                        context.Saga.TotalAmount,
                        context.Saga.PaymentMethod,
                        context.Saga.OrderShares));
                })
                .IfElse(context => context.Saga.PaymentMethod == "cod",
                    cod => cod
                        .Then(context =>
                        {
                            IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<IOutboxWriter>();
                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                outboxWriter.Enqueue(new OrderPaymentSucceeded(context.Saga.CorrelationId, orderId));
                            }
                        })
                        .TransitionTo(Completed),
                    vnpay => vnpay.TransitionTo(AwaitingPayment)),
            When(VoucherRedemptionFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        outboxWriter.Enqueue(new ReleaseStockCommand(orderId));
                    }

                    foreach (Guid orderId in context.Saga.OrderIds)
                    {
                        outboxWriter.Enqueue(new CancelOrder(
                            orderId, context.Saga.FailReason ?? "voucher redemption failed", "system"));
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
                        .Then(context =>
                        {
                            IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<IOutboxWriter>();
                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                outboxWriter.Enqueue(new OrderPaymentSucceeded(context.Saga.CorrelationId, orderId));
                            }
                        })
                        .TransitionTo(Completed),
                    failed => failed
                        .Then(context => context.Saga.FailReason = context.Message.Reason)
                        .Then(context =>
                        {
                            IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                                .GetRequiredService<IOutboxWriter>();

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                outboxWriter.Enqueue(new OrderPaymentFailed(
                                    context.Saga.CorrelationId, orderId, context.Saga.FailReason ?? "vnpay failed"));
                            }

                            if (context.Saga.VoucherRedeemed)
                            {
                                outboxWriter.Enqueue(new ReleaseVoucher(context.Saga.CorrelationId,
                                    context.Saga.OrderIds));
                            }

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                outboxWriter.Enqueue(new ReleaseStockCommand(orderId));
                            }

                            foreach (Guid orderId in context.Saga.OrderIds)
                            {
                                outboxWriter.Enqueue(new CancelOrder(
                                    orderId, context.Saga.FailReason ?? "vnpay failed", "system"));
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
