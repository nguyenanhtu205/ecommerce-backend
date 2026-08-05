namespace OrderService.Domain.Entities;

public class OrderShippingSnapshot
{
    public required Guid OrderId { get; init; }

    public required Guid CarrierId { get; init; }

    public required string CarrierName { get; init; }

    public required int Fee { get; init; }

    public DateOnly? EstimatedDeliveryStart { get; init; }

    public DateOnly? EstimatedDeliveryEnd { get; init; }

    public int? LateDeliveryCompensation { get; init; }

    public Order? Order { get; init; }
}
