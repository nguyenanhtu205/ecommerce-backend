namespace ShippingService.Domain.Entities;

public class Carrier : BaseEntity
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public ICollection<Shipment> Shipments { get; private set; } = new List<Shipment>();

    public ICollection<PickupPoint> PickupPoints { get; private set; } = new List<PickupPoint>();
}
