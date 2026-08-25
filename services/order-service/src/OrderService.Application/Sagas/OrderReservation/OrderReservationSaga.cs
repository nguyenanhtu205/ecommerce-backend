using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Sagas.OrderReservation;

public class OrderReservationSaga : MassTransitStateMachine<OrderReservationSagaState>
{
    public OrderReservationSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReserveOrderStockEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => StockReservedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderPaymentSucceededEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => ShipmentCreatedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => ShipmentCreationFailedEvent, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => CancelOrderEvent, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(ReserveOrderStockEvent)
                .Then(context =>
                {
                    context.Saga.CheckoutBatchId = context.Message.CheckoutBatchId;
                    context.Saga.CarrierId = context.Message.CarrierId;
                    context.Saga.PickupAddressSnapshot =
                        AddressMapper.ToAddressSnapshot(context.Message.PickupAddressSnapshot);
                    context.Saga.DeliveryAddressSnapshot =
                        AddressMapper.ToAddressSnapshot(context.Message.DeliveryAddressSnapshot);
                    context.Saga.Items = context.Message.Items;
                })
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();
                    outboxWriter.Enqueue(new ReserveStock(
                        context.Saga.CorrelationId,
                        [.. context.Saga.Items.Select(i => new ReserveStockItem(i.CombinationId, i.Quantity))]));
                })
                .TransitionTo(AwaitingReservation)
        );

        During(AwaitingReservation,
            When(StockReservedEvent)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();
                    outboxWriter.Enqueue(new OrderStockReserved(context.Saga.CheckoutBatchId,
                        context.Saga.CorrelationId));
                })
                .TransitionTo(AwaitingPaymentConfirmation),
            When(StockReservationFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();
                    outboxWriter.Enqueue(new OrderStockReservationFailed(
                        context.Saga.CheckoutBatchId, context.Saga.CorrelationId,
                        context.Saga.FailReason ?? "stock reservation failed"));
                })
                .TransitionTo(Cancelled)
        );

        During(AwaitingPaymentConfirmation,
            When(OrderPaymentSucceededEvent)
                .Then(context =>
                {
                    IOutboxWriter outboxWriter = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<IOutboxWriter>();

                    outboxWriter.Enqueue(new CommitStockCommand(context.Saga.CorrelationId));

                    outboxWriter.Enqueue(new CreateShipment(
                        context.Saga.CorrelationId,
                        AddressMapper.ToCheckoutAddressSnapshot(context.Saga.PickupAddressSnapshot!),
                        AddressMapper.ToCheckoutAddressSnapshot(context.Saga.DeliveryAddressSnapshot!),
                        context.Saga.CarrierId));
                })
                .TransitionTo(AwaitingShipment)
        );

        During(AwaitingShipment,
            When(ShipmentCreatedEvent)
                .TransitionTo(Completed),
            When(ShipmentCreationFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
        );

        DuringAny(
            When(CancelOrderEvent)
                .Then(context => context.Saga.FailReason ??= context.Message.Reason)
                .TransitionTo(Cancelled)
        );

        SetCompletedWhenFinalized();
    }

    public State AwaitingReservation { get; } = null!;
    public State AwaitingPaymentConfirmation { get; } = null!;
    public State AwaitingShipment { get; } = null!;
    public State Completed { get; } = null!;
    public State Cancelled { get; } = null!;

    public Event<ReserveOrderStock> ReserveOrderStockEvent { get; } = null!;
    public Event<StockReserved> StockReservedEvent { get; } = null!;
    public Event<StockReservationFailed> StockReservationFailedEvent { get; } = null!;
    public Event<OrderPaymentSucceeded> OrderPaymentSucceededEvent { get; } = null!;
    public Event<ShipmentCreated> ShipmentCreatedEvent { get; } = null!;
    public Event<ShipmentCreationFailed> ShipmentCreationFailedEvent { get; } = null!;
    public Event<CancelOrder> CancelOrderEvent { get; } = null!;
}
