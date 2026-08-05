namespace ShippingService.Domain.Entities;

public class Shipment : BaseEntity
{
    public required Guid OrderId { get; init; }

    public required Guid CarrierId { get; init; }

    public string? TrackingCode { get; init; }

    public required ShipmentStatus Status { get; init; }

    public required AddressSnapshot PickupAddressSnapshot { get; init; }

    public required AddressSnapshot DeliveryAddressSnapshot { get; init; }

    public DateOnly? EstimatedDeliveryStart { get; init; }

    public DateOnly? EstimatedDeliveryEnd { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public Carrier? Carrier { get; init; }

    public ICollection<ShipmentStatusHistory> ShipmentStatusHistories { get; private set; } =
        new List<ShipmentStatusHistory>();
}
