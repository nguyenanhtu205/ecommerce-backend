namespace ShippingService.Domain.Entities;

public class ShipmentStatusHistory : BaseEntity
{
    public required Guid ShipmentId { get; init; }

    public required ShipmentStatus Status { get; init; }

    public string? Location { get; init; }

    public required DateTimeOffset ChangedAt { get; init; }

    public Shipment? Shipment { get; init; }
}
