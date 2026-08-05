namespace OrderService.Application.Sagas.OrderReservation;

public class OrderReservationSagaState : SagaStateMachineInstance
{
    public required string CurrentState { get; set; }

    public Guid CheckoutBatchId { get; set; }

    public Guid CarrierId { get; set; }

    public AddressSnapshot? PickupAddressSnapshot { get; set; }

    public AddressSnapshot? DeliveryAddressSnapshot { get; set; }

    public List<OrderReadyItem> Items { get; set; } = [];

    public string? FailReason { get; set; }

    public Guid CorrelationId { get; set; }
}
