namespace ShippingService.Domain.Entities;

public class PickupPoint : BaseEntity
{
    public required Guid CarrierId { get; init; }

    public required string Name { get; init; }

    public required string Address { get; init; }

    public Carrier? Carrier { get; init; }
}
