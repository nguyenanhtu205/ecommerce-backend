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
                .ThenAsync(async context =>
                {
                    ITopicProducer<ReserveStock> producer = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<ITopicProducer<ReserveStock>>();
                    await producer.Produce(
                        new ReserveStock(
                            context.Saga.CorrelationId,
                            [.. context.Saga.Items.Select(i => new ReserveStockItem(i.CombinationId, i.Quantity))]),
                        context.CancellationToken);
                })
                .TransitionTo(AwaitingReservation)
        );

        During(AwaitingReservation,
            When(StockReservedEvent)
                .ThenAsync(async context =>
                {
                    ITopicProducer<OrderStockReserved> producer = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<ITopicProducer<OrderStockReserved>>();
                    await producer.Produce(
                        new OrderStockReserved(context.Saga.CheckoutBatchId, context.Saga.CorrelationId),
                        context.CancellationToken);
                })
                .TransitionTo(AwaitingPaymentConfirmation),
            When(StockReservationFailedEvent)
                .Then(context => context.Saga.FailReason = context.Message.Reason)
                .ThenAsync(async context =>
                {
                    ITopicProducer<OrderStockReservationFailed> producer = context.GetPayload<IServiceProvider>()
                        .GetRequiredService<ITopicProducer<OrderStockReservationFailed>>();
                    await producer.Produce(
                        new OrderStockReservationFailed(
                            context.Saga.CheckoutBatchId,
                            context.Saga.CorrelationId,
                            context.Saga.FailReason ?? "stock reservation failed"),
                        context.CancellationToken);
                })
                .TransitionTo(Cancelled)
        );

        During(AwaitingPaymentConfirmation,
            When(OrderPaymentSucceededEvent)
                .ThenAsync(async context =>
                {
                    IServiceProvider provider = context.GetPayload<IServiceProvider>();

                    ITopicProducer<CommitStockCommand> commitStockProducer = provider
                        .GetRequiredService<ITopicProducer<CommitStockCommand>>();
                    ITopicProducer<CreateShipment> createShipmentProducer = provider
                        .GetRequiredService<ITopicProducer<CreateShipment>>();

                    await commitStockProducer.Produce(
                        new CommitStockCommand(context.Saga.CorrelationId),
                        context.CancellationToken);

                    await createShipmentProducer.Produce(
                        new CreateShipment(
                            context.Saga.CorrelationId,
                            AddressMapper.ToCheckoutAddressSnapshot(context.Saga.PickupAddressSnapshot!),
                            AddressMapper.ToCheckoutAddressSnapshot(context.Saga.DeliveryAddressSnapshot!),
                            context.Saga.CarrierId),
                        context.CancellationToken);
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
